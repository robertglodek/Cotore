namespace Cotore.CORS;

public sealed class CorsOptions
{
    public bool Enabled { get; init; }
    public bool AllowCredentials { get; init; }
    public IEnumerable<string>? AllowedOrigins { get; init; }
    public IEnumerable<string>? AllowedMethods { get; init; }
    public IEnumerable<string>? AllowedHeaders { get; init; }
    public IEnumerable<string>? ExposedHeaders { get; init; }
}
