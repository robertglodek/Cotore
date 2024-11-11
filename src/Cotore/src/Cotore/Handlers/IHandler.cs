namespace Cotore.Handlers;

public interface IHandler
{
    Task HandleAsync(HttpContext context, RouteConfig config);
}