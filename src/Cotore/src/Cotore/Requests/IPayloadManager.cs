namespace Cotore.Requests;

internal interface IPayloadManager
{
    string GetKey(string? method, string upstream);
    IDictionary<string, PayloadSchema> Payloads { get; }
}