using Cotore.Exceptions;
using Cotore.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;

namespace Cotore.Metrics.OpenTelemetry;

public static class Extensions
{
    private const string ConsoleExporter = "console";
    private const string PrometheusExporter = "prometheus";
    private const string DefaultSectionName = "metrics";
    private const string RegistryKey = "metrics.openTelemetry";

    public static ICotoreBuilder AddMetrics(this ICotoreBuilder builder, string sectionName = DefaultSectionName)
    {
        sectionName = string.IsNullOrWhiteSpace(sectionName) ? DefaultSectionName : sectionName;

        var section = builder.Configuration.GetSection(sectionName);
        var options = section.BindOptions<MetricsOptions>();
        builder.Services.Configure<MetricsOptions>(section);

        if (!options.Enabled || !builder.TryRegister(RegistryKey))
        {
            return builder;
        }

        builder.Services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddHttpClientInstrumentation();
                builder.AddRuntimeInstrumentation();
                ConfigureExporter(builder, sectionName, options);
            });

        return builder;
    }

    private static void ConfigureExporter(MeterProviderBuilder builder, string sectionName, MetricsOptions options)
    {
        if(string.IsNullOrEmpty(options.Exporter))
        {
            throw new ConfigurationException("Metrics explorer cannot be empty.", PropertyPathHelper.GetOptionsPropertyPath<MetricsOptions>(sectionName, n => n.Exporter));
        }

        switch (options.Exporter.ToLowerInvariant())
        {
            case ConsoleExporter:
                builder.AddConsoleExporter();
                break;
            case PrometheusExporter:
                builder.AddPrometheusExporter(prometheus =>
                    prometheus.ScrapeEndpointPath = string.IsNullOrWhiteSpace(options.Endpoint) ? prometheus.ScrapeEndpointPath : options.Endpoint);
                break;
            default:
                throw new ConfigurationException($"Metrics explorer '{options.Exporter}' not configured.",
                    PropertyPathHelper.GetOptionsPropertyPath<MetricsOptions>(sectionName, n => n.Exporter));
        }
    }

    public static IApplicationBuilder UseMetrics(this IApplicationBuilder app)
    {
        var metricsOptions = app.ApplicationServices.GetRequiredService<IOptions<MetricsOptions>>().Value;
        if (!metricsOptions.Enabled)
        {
            return app;
        }

        if (metricsOptions.Exporter.ToLowerInvariant() is not PrometheusExporter)
        {
            return app;
        }

        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        return app;
    }
}