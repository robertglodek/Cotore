namespace Cotore.Requests;

public interface IPayloadBuilder
{
    Task<string> BuildRawAsync(HttpRequest request);
}