namespace Cotore.Tracing.OpenTelemetry;

public sealed class TracingOptions
{
    public bool Enabled { get; init; }
    public string Exporter { get; init; } = string.Empty;
    public JaegerOptions Jaeger { get; init; } = new();

    public sealed class JaegerOptions
    {
        public string AgentHost { get; init; } = "localhost";
        public int AgentPort { get; init; } = 6831;
        public int? MaxPayloadSizeInBytes { get; init; }
        public string ExportProcessorType { get; init; } = "batch";
    }
}