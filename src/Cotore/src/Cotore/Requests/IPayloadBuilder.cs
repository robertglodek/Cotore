using Microsoft.AspNetCore.Http;

namespace Cotore.Requests;

public interface IPayloadBuilder
{
    Task<string> BuildRawAsync(HttpRequest request);
}