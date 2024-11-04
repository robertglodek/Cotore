using Microsoft.AspNetCore.Http;

namespace Cotore.Hooks;

public interface IRequestHook
{
    Task InvokeAsync(HttpRequest request, ExecutionData data, CancellationToken cancellationToken = default);
}