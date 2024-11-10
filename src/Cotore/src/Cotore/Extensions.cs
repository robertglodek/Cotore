using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Cotore.Auth;
using Cotore.Handlers;
using Cotore.Options;
using Cotore.Requests;
using Cotore.Routing;
using Cotore.WebApi;
using Polly;
using Polly.Extensions.Http;
using System.Security.Cryptography.X509Certificates;
using Figgle;
using System.Text.Json.Serialization;
using System.Text.Json;
using Cotore.Exceptions;
using Microsoft.AspNetCore.Http.Json;
using Cotore.Serialization;

namespace Cotore;

public static class Extensions
{
    private const string DefaultAppSectionName = "app";

    public static IServiceCollection AddCotore(this IServiceCollection services,
        IConfiguration configuration,
        Action<ICotoreBuilder>? configure = null,
        string appSectionName = DefaultAppSectionName,
        string? cotoreOptionsSectionName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        appSectionName = string.IsNullOrWhiteSpace(appSectionName) ? DefaultAppSectionName : appSectionName;

        var builder = CotoreBuilder.Create(services, configuration);

        // Configure AppOptions
        var appOptionsSection = builder.Configuration.GetSection(appSectionName);
        builder.Services.Configure<AppOptions>(appOptionsSection);
        var appOptions = appOptionsSection.BindOptions<AppOptions>();

        // Configure CotoreOptions
        var cotoreOptionsSection = string.IsNullOrWhiteSpace(cotoreOptionsSectionName)
            ? builder.Configuration
            : builder.Configuration.GetSection(cotoreOptionsSectionName);

        builder.Services.Configure<CotoreOptions>(cotoreOptionsSection);
        var cotoreOptions = cotoreOptionsSection.BindOptions<CotoreOptions>();

        builder.Services.AddMvcCore().AddApiExplorer();
        services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        builder.Services.Configure<JsonOptions>(jsonOptions =>
        {
            jsonOptions.SerializerOptions.PropertyNameCaseInsensitive = true;
            jsonOptions.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonOptions.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            jsonOptions.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        builder.Services.ConfigureHttpClient(cotoreOptions);
        builder.Services.ConfigurePayloads(cotoreOptions);

        builder.Services.AddSingleton<IAuthenticationManager, AuthenticationManager>();
        builder.Services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
        builder.Services.AddSingleton<IPolicyManager, PolicyManager>();
        builder.Services.AddSingleton<IDownstreamBuilder, DownstreamBuilder>();
        builder.Services.AddSingleton<IPayloadBuilder, PayloadBuilder>();
        builder.Services.AddSingleton<IPayloadManager, PayloadManager>();
        builder.Services.AddSingleton<IPayloadTransformer, PayloadTransformer>();
        builder.Services.AddSingleton<IPayloadValidator, PayloadValidator>();
        builder.Services.AddSingleton<IRequestExecutionValidator, RequestExecutionValidator>();
        builder.Services.AddSingleton<IRequestHandlerManager, RequestHandlerManager>();
        builder.Services.AddSingleton<IRequestProcessor, RequestProcessor>();
        builder.Services.AddSingleton<IRouteConfigurator, RouteConfigurator>();
        builder.Services.AddSingleton<IRouteProvider, RouteProvider>();
        builder.Services.AddSingleton<ISchemaValidator, SchemaValidator>();
        builder.Services.AddSingleton<IValueProvider, ValueProvider>();
        builder.Services.AddSingleton<DownstreamHandler>();
        builder.Services.AddSingleton<ReturnValueHandler>();
        builder.Services.AddSingleton<WebApiEndpointDefinitions>();

        configure?.Invoke(builder);

        // Display App Name using Figgle Doom font
        Console.WriteLine(FiggleFonts.Doom.Render(appOptions.Name ?? "App"));

        return services;
    }

    private static IServiceCollection ConfigureHttpClient(this IServiceCollection services, CotoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Http.Name))
        {
            throw new CotoreConfigurationException("HTTP client name cannot be empty.");
        }

        var httpClientBuilder = services
          .AddHttpClient(options.Http.Name)
          .AddTransientHttpErrorPolicy(_ => HttpPolicyExtensions.HandleTransientHttpError()
              .WaitAndRetryAsync(options.Http.Resiliency.Retries, retry =>
                  options.Http.Resiliency.Exponential
                      ? TimeSpan.FromSeconds(Math.Pow(2, retry))
                      : options.Http.Resiliency.RetryInterval ?? TimeSpan.FromSeconds(2)));

        var certificateLocation = options.Http.Certificate?.Location;
        if (options.Http.Certificate is null || string.IsNullOrWhiteSpace(certificateLocation)) return services;
        var certificate = new X509Certificate2(certificateLocation, options.Http.Certificate.Password);
        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(certificate);
            return handler;
        });

        return services;
    }

    private static IServiceCollection ConfigurePayloads(this IServiceCollection services, CotoreOptions options)
    {
        options.PayloadsFolder ??= "payloads";

        if (options.PayloadsFolder.EndsWith('/'))
        {
            options.PayloadsFolder = options.PayloadsFolder[..^1];
        }

        return services;
    }

    public static IApplicationBuilder UseCotore(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<CotoreOptions>>().Value;

        if (options.Auth?.Enabled == true || options.Modules.Any(module => module.Value.Routes.Any(route => route.Auth == true)))
        {
            app.UseAuthentication();
        }

        if (options.UseForwardedHeaders)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
        }

        app.RegisterHandlers();

        app.AddRoutes();

        return app;
    }

    private static IApplicationBuilder RegisterHandlers(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<CotoreOptions>>().Value;
        var requestHandlerManager = app.ApplicationServices.GetRequiredService<IRequestHandlerManager>();
        requestHandlerManager.AddHandler("downstream", app.ApplicationServices.GetRequiredService<DownstreamHandler>());
        requestHandlerManager.AddHandler("return_value", app.ApplicationServices.GetRequiredService<ReturnValueHandler>());

        var handlers = options.Modules
            .Select(m => m.Value)
            .SelectMany(m => m.Routes)
            .Select(r => r.Use)
            .Distinct()
            .ToArray();

        foreach (var handler in handlers)
        {
            if (requestHandlerManager.Get(handler) is null)
            {
                throw new CotoreConfigurationException($"Handler: '{handler}' was not defined.");
            }
        }

        return app;
    }

    private static IApplicationBuilder AddRoutes(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<CotoreOptions>>().Value;
        ValidateRouteMethods(options);

        var routeProvider = app.ApplicationServices.GetRequiredService<IRouteProvider>();
        app.UseRouting();
        app.UseEndpoints(routeProvider.Build());

        return app;
    }
    
    private static void ValidateRouteMethods(CotoreOptions options)
    {
        foreach (var route in options.Modules.SelectMany(m => m.Value.Routes))
        {
            if (route.Methods.Count == 0) continue;

            if (route.Methods.Any(m => m.Equals(route.Method, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new CotoreConfigurationException($"The method '{route.Method.ToUpperInvariant()}'" +
                                                       $" is already defined in both the 'methods' collection" +
                                                       $" and as the 'method' for upstream '{route.Upstream}'.");
            }
        }
    }

    public static IApplicationBuilder UseRequestHandler<T>(this IApplicationBuilder app, string name)
        where T : IHandler
    {
        var requestHandlerManager = app.ApplicationServices.GetRequiredService<IRequestHandlerManager>();
        var handler = app.ApplicationServices.GetRequiredService<T>();
        requestHandlerManager.AddHandler(name, handler);

        return app;
    }

    public static bool IsNotEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.IsEmpty();

    public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable is null || !enumerable.Any();

    public static T BindOptions<T>(this IConfiguration configuration, string sectionName) where T : new()
        => BindOptions<T>(configuration.GetSection(sectionName));

    public static T BindOptions<T>(this IConfigurationSection section) where T : new()
    {
        var options = new T();
        section.Bind(options);
        return options;
    }

    public static T BindOptions<T>(this IConfiguration section) where T : new()
    {
        var options = new T();
        section.Bind(options);
        return options;
    }
}