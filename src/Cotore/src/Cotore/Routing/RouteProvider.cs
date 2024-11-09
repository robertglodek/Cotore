using Microsoft.AspNetCore.Builder;
using Cotore.Options;
using Cotore.WebApi;
using Cotore.Requests;

namespace Cotore.Routing;

internal sealed class RouteProvider : IRouteProvider
{
    private readonly IDictionary<string, Action<IEndpointRouteBuilder, string, RouteConfig>> _methods;
    private readonly IRouteConfigurator _routeConfigurator;
    private readonly IRequestExecutionValidator _requestExecutionValidator;
    private readonly IUpstreamBuilder _upstreamBuilder;
    private readonly WebApiEndpointDefinitions _definitions;
    private readonly CotoreOptions _options;
    private readonly IRequestHandlerManager _requestHandlerManager;

    public RouteProvider(IOptions<CotoreOptions> options,
        IRequestHandlerManager requestHandlerManager,
        IRouteConfigurator routeConfigurator,
        IRequestExecutionValidator requestExecutionValidator,
        IUpstreamBuilder upstreamBuilder,
        WebApiEndpointDefinitions definitions)
    {
        _routeConfigurator = routeConfigurator;
        _requestExecutionValidator = requestExecutionValidator;
        _upstreamBuilder = upstreamBuilder;
        _definitions = definitions;
        _options = options.Value;
        _requestHandlerManager = requestHandlerManager;
        _methods = new Dictionary<string, Action<IEndpointRouteBuilder, string, RouteConfig>>
        {
            ["get"] = (builder, path, routeConfig) =>
                builder.MapGet(path, ctx => Handle(ctx, routeConfig)),
            ["post"] = (builder, path, routeConfig) =>
                builder.MapPost(path, ctx => Handle(ctx, routeConfig)),
            ["put"] = (builder, path, routeConfig) =>
                builder.MapPut(path, ctx => Handle(ctx, routeConfig)),
            ["delete"] = (builder, path, routeConfig) =>
                builder.MapDelete(path, ctx => Handle(ctx, routeConfig)),
        };
    }

    private async Task Handle(HttpContext context, RouteConfig routeConfig)
    {
        var skipAuth = _options.Auth?.Enabled != true && routeConfig.Route?.Auth != true;

        if (!skipAuth && !await _requestExecutionValidator.TryExecuteAsync(context, routeConfig))
        {
            return;
        }

        var handler = routeConfig.Route!.Use;
        await _requestHandlerManager.HandleAsync(handler, context, routeConfig);
    }

    public Action<IEndpointRouteBuilder> Build() => routeBuilder =>
    {
        foreach (var module in _options.Modules.Where(m => m.Value.Enabled != false))
        {
            foreach (var route in module.Value.Routes)
            {
                if (string.IsNullOrWhiteSpace(route.Method) && (route.Methods is null || route.Methods.Count == 0))
                {
                    throw new ArgumentException("Both, route 'method' and 'methods' cannot be empty.");
                }

                route.Upstream = _upstreamBuilder.Build(module.Value, route);
                var routeConfig = _routeConfigurator.Configure(module.Value, route);

                if (!string.IsNullOrWhiteSpace(route.Method))
                {
                    _methods[route.Method](routeBuilder, route.Upstream, routeConfig);
                    AddEndpointDefinition(route.Method, route.Upstream);
                }

                if (route.Methods is null)
                {
                    continue;
                }

                foreach (var methodType in route.Methods.Select(method => method.ToLowerInvariant()))
                {
                    _methods[methodType](routeBuilder, route.Upstream, routeConfig);
                    AddEndpointDefinition(methodType, route.Upstream);
                }
            }
        }
    };
    
    private void AddEndpointDefinition(string method, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "/";
        }
        
        _definitions.Add(new WebApiEndpointDefinition
        {
            Method = method,
            Path = path,
            Responses =
            [
                new WebApiEndpointResponse
                {
                    StatusCode = 200
                }
            ]
        });
    }
}