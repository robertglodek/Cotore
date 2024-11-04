using Cotore.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Cotore.Routing;

internal sealed class RequestExecutionValidator(IAuthenticationManager authenticationManager,
    IAuthorizationManager authorizationManager, ILogger<RequestExecutionValidator> logger) : IRequestExecutionValidator
{
    private readonly IAuthenticationManager _authenticationManager = authenticationManager;
    private readonly IAuthorizationManager _authorizationManager = authorizationManager;
    private readonly ILogger<RequestExecutionValidator> _logger = logger;

    public async Task<bool> TryExecuteAsync(HttpContext context, RouteConfig routeConfig)
    {
        var activity = Activity.Current;
        var isAuthenticated = await _authenticationManager.TryAuthenticateAsync(context.Request, routeConfig);
        if (!isAuthenticated)
        {
            _logger.LogWarning("Unauthorized request to: {UpstreamRoute} [Activity ID: {ActivityId}]", routeConfig.Route.Upstream, activity!.Id);
            context.Response.StatusCode = 401;
            return false;
        }

        if (_authorizationManager.IsAuthorized(context.User, routeConfig))
        {
            return true;
        }

        _logger.LogWarning("Forbidden request to: {UpstreamRoute} by user: {UserName} [Activity ID: {ActivityId}]",
            routeConfig.Route.Upstream, context.User?.Identity?.Name, activity!.Id);
        context.Response.StatusCode = 403;

        return false;
    }
}