namespace Cotore.Configuration;

public sealed class HttpOptions
{
    public string Name { get; init; } = "cotore";
    public CertificateOptions? Certificate { get; init; }
    public ResiliencyOptions Resiliency { get; init; } = new();

    [UsedImplicitly]
    public sealed class CertificateOptions
    {
        public string Location { get; init; } = string.Empty;
        public string? Password { get; init; }
    }

    public sealed class ResiliencyOptions
    {
        public int Retries { get; init; } = 3;
        public TimeSpan? RetryInterval { get; init; }
        public bool Exponential { get; init; }
    }
}