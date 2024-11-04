using Microsoft.AspNetCore.Http;

namespace Cotore.Hooks;

public interface IResponseHook
{
    Task InvokeAsync(HttpResponse response, ExecutionData data, CancellationToken cancellationToken = default);
}