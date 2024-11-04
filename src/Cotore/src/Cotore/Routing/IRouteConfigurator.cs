using Cotore.Configuration;

namespace Cotore.Routing;

internal interface IRouteConfigurator
{
    RouteConfig Configure(ModuleOptions module, RouteOptions route);
}