using Cotore.Configuration;

namespace Cotore.Routing;

internal interface IUpstreamBuilder
{
    string Build(ModuleOptions module, Configuration.RouteOptions route);
}