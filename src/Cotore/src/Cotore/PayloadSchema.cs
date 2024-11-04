using System.Dynamic;

namespace Cotore;

public class PayloadSchema(ExpandoObject payload, string? schema)
{
    public ExpandoObject Payload { get; } = payload;
    public string? Schema { get; } = schema;
}