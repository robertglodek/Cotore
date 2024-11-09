namespace Cotore.Metrics.OpenTelemetry;

public sealed class MetricsOptions
{
    public bool Enabled { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string Exporter { get; init; } = string.Empty;
}