namespace Cotore;

public sealed class PayloadSchema(ExpandoObject payload, string? schema)
{
    public ExpandoObject Payload { get; } = payload;
    public string? Schema { get; } = schema;
}