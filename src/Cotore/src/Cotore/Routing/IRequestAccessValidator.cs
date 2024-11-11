namespace Cotore.Routing;

internal interface IRequestAccessValidator
{
    Task<bool> ValidateAsync(HttpContext context, RouteConfig routeConfig);
}