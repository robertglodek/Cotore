using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cotore.CORS;

public static class Extensions
{
    private const string DefaultSectionName = "cors";
    private const string RegistryName = "cors";

    public static ICotoreBuilder AddCorsPolicy(this ICotoreBuilder builder, string sectionName = DefaultSectionName)
    {
        sectionName = !string.IsNullOrWhiteSpace(sectionName) ? sectionName : DefaultSectionName;   

        var section = builder.Configuration.GetSection(sectionName);
        var options = section.BindOptions<CorsOptions>();
        builder.Services.Configure<CorsOptions>(section);

        if (!options.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services.AddCors(cors =>
        {
            var allowedHeaders = options.AllowedHeaders;
            var allowedMethods = options.AllowedMethods;
            var allowedOrigins = options.AllowedOrigins;
            var exposedHeaders = options.ExposedHeaders;
            cors.AddPolicy(RegistryName, corsBuilder =>
            {
                var origins = allowedOrigins?.ToArray() ?? [];
                if (options.AllowCredentials && origins.FirstOrDefault() != "*")
                {
                    corsBuilder.AllowCredentials();
                }
                else
                {
                    corsBuilder.DisallowCredentials();
                }

                corsBuilder
                    .WithHeaders(allowedHeaders?.ToArray() ?? [])
                    .WithMethods(allowedMethods?.ToArray() ?? [])
                    .WithOrigins([.. origins])
                    .WithExposedHeaders(exposedHeaders?.ToArray() ?? []);
            });
        });

        return builder;
    }

    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<CorsOptions>>().Value;
        if (!options.Enabled)
        {
            return app;
        }

        app.UseCors(RegistryName);

        return app;
    }
}
