namespace Cotore.Routing;

internal interface IRequestExecutionValidator
{
    Task<bool> TryExecuteAsync(HttpContext context, RouteConfig routeConfig);
}