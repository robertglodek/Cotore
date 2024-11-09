namespace Cotore.Swagger;

public sealed class SwaggerOptions
{
    public bool Enabled { get; init; } = true;
    public bool ReDocEnabled { get; init; }
    public string Name { get; init; } = "Gateway API v1";
    public string Title { get; init; } = "Gateway API Documentation";
    public string Version { get; init; } = "1.0";
    public string? RoutePrefix { get; init; }
    public bool IncludeSecurity { get; init; } = true;
}

