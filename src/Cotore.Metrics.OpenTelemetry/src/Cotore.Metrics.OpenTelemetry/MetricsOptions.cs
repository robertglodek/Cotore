namespace Cotore.Metrics.OpenTelemetry;

public sealed class MetricsOptions
{
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Exporter { get; set; } = string.Empty;
}