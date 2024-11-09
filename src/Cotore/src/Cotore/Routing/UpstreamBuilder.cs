using Cotore.Configuration;

namespace Cotore.Routing;

internal sealed class UpstreamBuilder : IUpstreamBuilder
{
    public string Build(ModuleOptions module, Configuration.RouteOptions route)
    {
        var path = module.Path;
        var upstream = string.IsNullOrWhiteSpace(route.Upstream) ? string.Empty : route.Upstream;
        if (!string.IsNullOrWhiteSpace(path))
        {
            var modulePath = path.EndsWith('/') ? path[..^1] : path;
            if (!upstream.StartsWith('/'))
            {
                upstream = $"/{upstream}";
            }

            upstream = $"{modulePath}{upstream}";
        }

        if (upstream.EndsWith('/'))
        {
            upstream = upstream[..^1];
        }

        if (route.MatchAll)
        {
            upstream = $"{upstream}/{{*url}}";
        }

        return upstream;
    }
}