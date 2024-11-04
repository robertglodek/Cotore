using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Elastic.Serilog.Sinks;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Ingest.Elasticsearch;
using Elastic.Transport;
using Cotore.Options;
using Cotore.Exceptions;

namespace Cotore.Logging.Serilog;

public static class Extensions
{
    internal static LoggingLevelSwitch LoggingLevelSwitch = new();
    private const string DefaultSectionName = "logger";
    private const string DefaultAppSectionName = "app";
    private const string ConsoleOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}";
    private const string FileOutputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}";
    private const string RegistryKey = "logging.serilog";

    public static ICotoreBuilder AddLogger(this ICotoreBuilder builder, string sectionName = DefaultSectionName)
    {
        sectionName = string.IsNullOrWhiteSpace(sectionName) ? DefaultSectionName : sectionName;

        var section = builder.Configuration.GetSection(sectionName);
        builder.Services.Configure<LoggerOptions>(section);

        if (!builder.TryRegister(RegistryKey))
        {
            return builder;
        }

        builder.Services.AddSingleton<ILoggingService, LoggingService>();

        return builder;
    }

    public static IHostBuilder UseLogging(this IHostBuilder hostBuilder,
        string loggerSectionName = DefaultSectionName,
        string appSectionName = DefaultAppSectionName,
        Action<HostBuilderContext, LoggerConfiguration>? configure = null) => hostBuilder
            .UseSerilog((context, loggerConfiguration) =>
            {
                loggerSectionName = string.IsNullOrWhiteSpace(loggerSectionName) ? DefaultSectionName : loggerSectionName;
                appSectionName = string.IsNullOrWhiteSpace(appSectionName) ? DefaultAppSectionName : appSectionName;

                var appOptions = context.Configuration.BindOptions<AppOptions>(appSectionName);
                var loggerOptions = context.Configuration.BindOptions<LoggerOptions>(loggerSectionName);

                MapOptions(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                configure?.Invoke(context, loggerConfiguration);
            });

    private static void MapOptions(
        LoggerOptions loggerOptions,
        AppOptions appOptions,
        LoggerConfiguration loggerConfiguration,
        string environmentName)
    {
        LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(loggerOptions.Level);

        loggerConfiguration.Enrich.FromLogContext()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("Application", appOptions.Service);

        foreach (var (key, value) in loggerOptions.Tags ?? new Dictionary<string, object>())
        {
            loggerConfiguration.Enrich.WithProperty(key, value);
        }

        foreach (var (key, value) in loggerOptions.MinimumLevelOverrides ?? new Dictionary<string, string>())
        {
            var logLevel = GetLogEventLevel(value);
            loggerConfiguration.MinimumLevel.Override(key, logLevel);
        }

        loggerOptions.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty<string>("RequestPath", n => n.EndsWith(p))));

        loggerOptions.ExcludeProperties?.ToList().ForEach(p => loggerConfiguration.Filter
            .ByExcluding(Matching.WithProperty(p)));

        Configure(loggerConfiguration, loggerOptions, appOptions);
    }

    private static void Configure(LoggerConfiguration loggerConfiguration,
        LoggerOptions options, AppOptions appOptions)
    {
        var consoleOptions = options.Console ?? new LoggerOptions.ConsoleOptions();
        var fileOptions = options.File ?? new LoggerOptions.FileOptions();
        var seqOptions = options.Seq ?? new LoggerOptions.SeqOptions();
        var mongoOptions = options.Mongo ?? new LoggerOptions.MongoDBOptions();
        var elkOptions = options.Elk ?? new LoggerOptions.ElkOptions();

        if (consoleOptions.Enabled)
        {
            var template = !string.IsNullOrEmpty(consoleOptions.Template) ? consoleOptions.Template : ConsoleOutputTemplate;
            loggerConfiguration.WriteTo.Console(outputTemplate: template);
        }

        if (fileOptions.Enabled)
        {
            var template = !string.IsNullOrEmpty(fileOptions.Template) ? fileOptions.Template : FileOutputTemplate;
            var path = string.IsNullOrWhiteSpace(fileOptions.Path) ? "logs/logs.txt" : fileOptions.Path;
            if (!Enum.TryParse<RollingInterval>(fileOptions.Interval, true, out var interval))
            {
                interval = RollingInterval.Day;
            }

            loggerConfiguration.WriteTo.File(path, rollingInterval: interval);
        }

        if (seqOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Seq(seqOptions.Url, apiKey: seqOptions.ApiKey);
        }

        if (mongoOptions.Enabled)
        {
            loggerConfiguration.WriteTo.MongoDBBson(cfg =>
            {
                cfg.SetMongoUrl(mongoOptions.Url);
                cfg.SetCollectionName(mongoOptions.Collection);
            });
        }

        if (elkOptions.Enabled)
        {
            loggerConfiguration.WriteTo.Elasticsearch([new Uri(elkOptions.Url)], opts =>
            {
                opts.DataStream = new DataStreamName("logs", appOptions.Service, appOptions.Name);
                opts.BootstrapMethod = BootstrapMethod.Failure;
            }, transport =>
            {
                if (elkOptions.BasicAuthEnabled)
                {
                    if (string.IsNullOrEmpty(elkOptions.Username))
                    {
                        throw new ConfigurationException("When Basic Authentication enabled, username must not be empty.", nameof(elkOptions.Username));
                    }

                    if (string.IsNullOrEmpty(elkOptions.Password))
                    {
                        throw new ConfigurationException("When Basic Authentication enabled, password must not be empty.", nameof(elkOptions.Password));
                    }

                    transport.Authentication(new BasicAuthentication(elkOptions.Username, elkOptions.Password));
                }
            });
        }
    }

    public static IEndpointConventionBuilder MapLogLevelHandler(this IEndpointRouteBuilder builder,
        string endpointRoute = "~/logging/level")
        => builder.MapPost(endpointRoute, LevelSwitch);

    internal static LogEventLevel GetLogEventLevel(string? level)
        => Enum.TryParse<LogEventLevel>(level, true, out var logLevel)
            ? logLevel
            : LogEventLevel.Information;

    private static async Task LevelSwitch(HttpContext context)
    {
        var service = context.RequestServices.GetService<ILoggingService>();
        if (service is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("ILoggingService is not registered. Add UseLogging() to your Program.cs.");
            return;
        }

        var level = context.Request.Query["level"].ToString();

        if (string.IsNullOrEmpty(level))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid value for logging level.");
            return;
        }

        service.SetLoggingLevel(level);

        context.Response.StatusCode = StatusCodes.Status200OK;
    }
}
