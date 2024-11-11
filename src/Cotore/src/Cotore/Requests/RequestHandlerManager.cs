using System.Collections.Concurrent;
using Cotore.Handlers;

namespace Cotore.Requests;

internal sealed class RequestHandlerManager(ILogger<RequestHandlerManager> logger) : IRequestHandlerManager
{
    private static readonly ConcurrentDictionary<string, IHandler> Handlers = new();

    public void AddHandler(string name, IHandler handler)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Handler name cannot be null or empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(handler);

        if (Handlers.TryAdd(name, handler))
        {
            logger.LogInformation("Added a request handler: '{HandlerName}'.", name);
        }
        else
        {
            logger.LogError("Couldn't add a request handler: '{HandlerName}'. Handler with the same name already exists.", name);
        }
    }

    public async Task HandleAsync(string handler, HttpContext context, RouteConfig routeConfig)
    {
        if (!Handlers.TryGetValue(handler, out var instance))
        {
            throw new Exception($"Handler: '{handler}' was not found.");
        }

        await instance.HandleAsync(context, routeConfig);
    }

    public IHandler? Get(string name) => Handlers.GetValueOrDefault(name);
}