using Cotore.Handlers;
using Microsoft.AspNetCore.Http;

namespace Cotore.Requests;

public interface IRequestHandlerManager
{
    IHandler? Get(string name);
    void AddHandler(string name, IHandler handler);
    Task HandleAsync(string handler, HttpContext context, RouteConfig routeConfig, CancellationToken cancellationToken = default);
}