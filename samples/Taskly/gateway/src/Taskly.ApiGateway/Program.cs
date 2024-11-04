using Cotore;
using Cotore.Auth.Jwt;
using Cotore.CORS;
using Cotore.Logging.Serilog;
using Cotore.Metrics.OpenTelemetry;
using Cotore.Swagger;
using Cotore.Tracing.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLogging();

builder.Configuration
    .AddJsonFile("cotore.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"cotore.{Environment.GetEnvironmentVariable("COTORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true);

builder.Services.AddCotore(builder.Configuration, configure =>
{
    configure.AddCorsPolicy();
    configure.AddJwt();
    configure.AddLogger();
    configure.AddSwaggerDocs();
    configure.AddTracing();
    configure.AddMetrics();
});

var app = builder.Build();

app.UseCorsPolicy();

app.UseMetrics();

app.UseCotore();

app.Run();