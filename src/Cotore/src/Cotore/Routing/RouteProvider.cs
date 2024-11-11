using Cotore.Options;
using Cotore.WebApi;
using Cotore.Requests;

namespace Cotore.Routing;

internal sealed class RouteProvider : IRouteProvider
{
    private readonly IDictionary<string, Action<IEndpointRouteBuilder, string, RouteConfig>> _methods;
    private readonly IRouteConfigurator _routeConfigurator;
    private readonly IRequestAccessValidator _requestAccessValidator;
    private readonly WebApiEndpointDefinitions _definitions;
    private readonly CotoreOptions _options;
    private readonly IRequestHandlerManager _requestHandlerManager;

    public RouteProvider(IOptions<CotoreOptions> options,
        IRequestHandlerManager requestHandlerManager,
        IRouteConfigurator routeConfigurator,
        IRequestAccessValidator requestAccessValidator,
        WebApiEndpointDefinitions definitions)
    {
        _routeConfigurator = routeConfigurator;
        _requestAccessValidator = requestAccessValidator;
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
            ["patch"] = (builder, path, routeConfig) =>
                builder.MapPatch(path, ctx => Handle(ctx, routeConfig)),
            ["delete"] = (builder, path, routeConfig) =>
                builder.MapDelete(path, ctx => Handle(ctx, routeConfig))
        };
    }

    private async Task Handle(HttpContext context, RouteConfig routeConfig)
    {
        if (!await _requestAccessValidator.ValidateAsync(context, routeConfig))
        {
            return;
        }

        var handler = routeConfig.Route.Use;
        await _requestHandlerManager.HandleAsync(handler, context, routeConfig);
    }

    public Action<IEndpointRouteBuilder> Build() => routeBuilder =>
    {
        foreach (var module in _options.Modules.Where(m => m.Value.Enabled != false))
        {
            foreach (var route in module.Value.Routes)
            {
                var routeConfig = _routeConfigurator.Configure(module.Value, route);

                if (!string.IsNullOrWhiteSpace(route.Method))
                {
                    _methods[route.Method.ToLowerInvariant()](routeBuilder, routeConfig.Upstream, routeConfig);
                    AddEndpointDefinition(route.Method, routeConfig.Upstream);
                }

                foreach (var methodType in route.Methods.Select(method => method.ToLowerInvariant()))
                {
                    _methods[methodType](routeBuilder, routeConfig.Upstream, routeConfig);
                    AddEndpointDefinition(methodType, routeConfig.Upstream);
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