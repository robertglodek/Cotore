using Cotore.Configuration;
using Cotore.Exceptions;
using Cotore.Options;
using RouteOptions = Cotore.Configuration.RouteOptions;

namespace Cotore.Routing;

internal sealed class RouteConfigurator(IOptions<CotoreOptions> options) : IRouteConfigurator
{
    public RouteConfig Configure(ModuleOptions module, RouteOptions route)
        => new()
        {
            Route = route,
            Downstream = GetDownstream(module, route),
            Upstream = GetUpstream(module, route)
        };

    private string? GetDownstream(ModuleOptions module, RouteOptions route)
    {
        if (string.IsNullOrWhiteSpace(route.Downstream))
        {
            return null;
        }

        var loadBalancerEnabled = options.Value.LoadBalancer?.Enabled == true;
        var loadBalancerUrl = options.Value.LoadBalancer?.Url;
        if (loadBalancerEnabled)
        {
            if (string.IsNullOrWhiteSpace(loadBalancerUrl))
            {
                throw new CotoreConfigurationException("Load balancer URL cannot be empty.");
            }

            loadBalancerUrl = loadBalancerUrl.EndsWith('/') ? loadBalancerUrl : $"{loadBalancerUrl}/";
        }

        var basePath = route.Downstream.Contains('/')
            ? route.Downstream.Split('/')[0]
            : route.Downstream;

        var hasService = module.Services.TryGetValue(basePath, out var service);
        if (!hasService)
        {
            return SetProtocol(route.Downstream);
        }

        if (service is null)
        {
            throw new ArgumentException($"Service for: '{basePath}' was not defined.", nameof(service.Url));
        }

        if (options.Value.UseLocalUrl)
        {
            if (string.IsNullOrWhiteSpace(service.LocalUrl))
            {
                throw new ArgumentException($"Local URL for: '{basePath}' cannot be empty if useLocalUrl = true.",
                    nameof(service.LocalUrl));
            }

            return SetProtocol(route.Downstream.Replace(basePath, service.LocalUrl));
        }

        if (!string.IsNullOrWhiteSpace(service.LocalUrl) && string.IsNullOrWhiteSpace(service.Url))
        {
            return SetProtocol(route.Downstream.Replace(basePath, service.LocalUrl));
        }

        if (string.IsNullOrWhiteSpace(service.Url))
        {
            throw new ArgumentException($"Service URL for: '{basePath}' cannot be empty.", nameof(service.Url));
        }

        if (!loadBalancerEnabled)
        {
            return SetProtocol(route.Downstream.Replace(basePath, service.Url));
        }

        var serviceUrl = service.Url.StartsWith('/') ? service.Url[1..] : service.Url;
        var loadBalancedServiceUrl = $"{loadBalancerUrl}{serviceUrl}";

        return SetProtocol(route.Downstream.Replace(basePath, loadBalancedServiceUrl));
    }

    private static string SetProtocol(string service) => service.StartsWith("http") ? service : $"http://{service}";
    
    
    private string GetUpstream(ModuleOptions module, RouteOptions route)
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