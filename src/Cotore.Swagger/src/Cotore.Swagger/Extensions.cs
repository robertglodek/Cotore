using Cotore.Swagger.Exceptions;
using Cotore.Swagger.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cotore.Swagger;

public static class Extensions
{
    private const string DefaultSectionName = "swagger";
    private const string RegistryKey = "webApi.swagger";

    public static ICotoreBuilder AddSwaggerDocs(this ICotoreBuilder builder, string sectionName = DefaultSectionName)
    {
        sectionName = string.IsNullOrWhiteSpace(sectionName) ? DefaultSectionName : sectionName;

        var section = builder.Configuration.GetSection(sectionName);
        var options = section.BindOptions<SwaggerOptions>();
        builder.Services.Configure<SwaggerOptions>(section);

        if (!options.Enabled || !builder.TryRegister(RegistryKey))
        {
            return builder;
        }

        ValidateSwaggerOptions(options);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            ConfigureSwaggerGen(c, options);
        });

        return builder;
    }

    public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder builder)
    {
        var options = builder.ApplicationServices.GetRequiredService<IOptions<SwaggerOptions>>().Value;
        if (!options.Enabled)
        {
            return builder;
        }

        var routePrefix = string.IsNullOrWhiteSpace(options.RoutePrefix) ? string.Empty : options.RoutePrefix;

        builder.UseSwagger(c =>
        {
            c.RouteTemplate = $"{routePrefix}/{{documentName}}/swagger.json";
        });

        return options.ReDocEnabled
            ? builder.UseReDoc(c =>
            {
                c.RoutePrefix = routePrefix;
                c.SpecUrl = $"{options.Name}/swagger.json";
            })
            : builder.UseSwaggerUI(c =>
            {
                c.RoutePrefix = routePrefix;
                c.SwaggerEndpoint($"/{routePrefix}/{options.Name}/swagger.json".FormatEmptyRoutePrefix(), options.Title);
            });
    }

    private static void ConfigureSwaggerGen(SwaggerGenOptions options, SwaggerOptions swaggerOptions)
    {
        options.DocumentFilter<WebApiDocumentFilter>();
        options.SwaggerDoc(swaggerOptions.Name, new OpenApiInfo { Title = swaggerOptions.Title, Version = swaggerOptions.Version });

        if (swaggerOptions.IncludeSecurity)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
        }
    }

    private static void ValidateSwaggerOptions(SwaggerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Name))
        {
            throw new SwaggerConfigurationException("Name cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            throw new SwaggerConfigurationException("Title cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(options.Version))
        {
            throw new SwaggerConfigurationException("Version cannot be null or whitespace.");
        }
    }

    private static string FormatEmptyRoutePrefix(this string route)
        => route.Replace("//", "/");
}