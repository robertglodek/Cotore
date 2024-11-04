namespace Cotore.Swagger;

public sealed class SwaggerOptions
{
    public bool Enabled { get; set; } = true;
    public bool ReDocEnabled { get; set; } = false;
    public string Name { get; set; } = "Gateway API v1";
    public string Title { get; set; } = "Gateway API Documentation";
    public string Version { get; set; } = "1.0";
    public string? RoutePrefix { get; set; } = null;
    public bool IncludeSecurity { get; set; } = true;
}

