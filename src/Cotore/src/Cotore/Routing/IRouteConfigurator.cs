using Cotore.Configuration;
using RouteOptions = Cotore.Configuration.RouteOptions;

namespace Cotore.Routing;

internal interface IRouteConfigurator
{
    RouteConfig Configure(ModuleOptions module, RouteOptions route);
}