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
                throw new CotoreConfigurationException("Load balancer URL must be set when load balancing is enabled.");
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
            throw new CotoreConfigurationException($"Service for: '{basePath}' was not defined.");
        }

        if (options.Value.UseLocalUrl)
        {
            if (string.IsNullOrWhiteSpace(service.LocalUrl))
            {
                throw new CotoreConfigurationException($"Local URL for: '{basePath}' cannot be empty if useLocalUrl = true.");
            }

            return SetProtocol(route.Downstream.Replace(basePath, service.LocalUrl));
        }

        if (!string.IsNullOrWhiteSpace(service.LocalUrl) && string.IsNullOrWhiteSpace(service.Url))
        {
            return SetProtocol(route.Downstream.Replace(basePath, service.LocalUrl));
        }

        if (string.IsNullOrWhiteSpace(service.Url))
        {
            throw new CotoreConfigurationException($"Service URL for: '{basePath}' cannot be empty.");
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
    
    private static string GetUpstream(ModuleOptions module, RouteOptions route)
    {
        var path = module.Path.TrimEnd('/');
        var upstream = route.Upstream switch
        {
            { Length: > 0 } => $"/{route.Upstream.TrimStart('/')}",
            _ => string.Empty
        };

        var combined = $"{path}{upstream}".TrimEnd('/');
        return route.MatchAll ? $"{combined}/{{*url}}" : combined;
    }
}