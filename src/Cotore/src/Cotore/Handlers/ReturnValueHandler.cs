using Cotore.Hooks;
using Cotore.Requests;

namespace Cotore.Handlers;

internal sealed class ReturnValueHandler(
    IRequestProcessor requestProcessor,
    IServiceProvider serviceProvider) : IHandler
{
    private readonly IEnumerable<IRequestHook> _requestHooks = serviceProvider.GetServices<IRequestHook>();
    private readonly IEnumerable<IResponseHook> _responseHooks = serviceProvider.GetServices<IResponseHook>();

    public async Task HandleAsync(HttpContext context, RouteConfig config, CancellationToken cancellationToken = default)
    {
        var executionData = await requestProcessor.ProcessAsync(config, context);

        foreach (var hook in _requestHooks)
        {
            await hook.InvokeAsync(context.Request, executionData, cancellationToken);
        }

        foreach (var hook in _responseHooks)
        {
            await hook.InvokeAsync(context.Response, executionData, cancellationToken);
        }

        var returnValue = config.Route.ReturnValue ?? string.Empty;

        await context.Response.WriteAsync(returnValue, cancellationToken);
    }
}