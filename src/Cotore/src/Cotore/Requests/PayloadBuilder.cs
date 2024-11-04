using Cotore.Serialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cotore.Requests;

internal sealed class PayloadBuilder : IPayloadBuilder
{
    public async Task<string> BuildRawAsync(HttpRequest request)
    {
        var content = string.Empty;
        if (request.Body == null)
        {
            return content;
        }

        using (var reader = new StreamReader(request.Body))
        {
            content = await reader.ReadToEndAsync();
        }

        return content;
    }
}