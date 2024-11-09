namespace Cotore.Routing;

internal interface IDownstreamBuilder
{
    string? GetDownstream(RouteConfig routeConfig, HttpRequest request, RouteData data);
}