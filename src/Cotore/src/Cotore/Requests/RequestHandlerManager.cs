using System.Collections.Concurrent;
using Cotore.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cotore.Requests;

internal sealed class RequestHandlerManager(ILogger<RequestHandlerManager> logger) : IRequestHandlerManager
{
    private readonly ILogger<IRequestHandlerManager> _logger = logger;
    private static readonly ConcurrentDictionary<string, IHandler> _handlers = new();

    public void AddHandler(string name, IHandler handler)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Handler name cannot be null or empty.", nameof(name));
        }
        
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
            
        if (_handlers.TryAdd(name, handler))
        {
            _logger.LogInformation("Added a request handler: '{HandlerName}'.", name);
        }
        else
        {
            _logger.LogError("Couldn't add a request handler: '{HandlerName}'. Handler with the same name already exists.", name);
        }
    }

    public async Task HandleAsync(string handler, HttpContext context, RouteConfig routeConfig, CancellationToken cancellationToken = default)
    {
        if (!_handlers.TryGetValue(handler, out var instance))
        {
            throw new Exception($"Handler: '{handler}' was not found.");
        }

        await instance.HandleAsync(context, routeConfig, cancellationToken);
    }

    public IHandler? Get(string name) => _handlers.TryGetValue(name, out var handler) ? handler : null;
}