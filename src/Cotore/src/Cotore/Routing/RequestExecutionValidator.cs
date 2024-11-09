using Cotore.Auth;
using System.Diagnostics;

namespace Cotore.Routing;

internal sealed class RequestExecutionValidator(IAuthenticationManager authenticationManager,
    IAuthorizationManager authorizationManager, ILogger<RequestExecutionValidator> logger) : IRequestExecutionValidator
{
    public async Task<bool> TryExecuteAsync(HttpContext context, RouteConfig routeConfig)
    {
        var activity = Activity.Current;
        var isAuthenticated = await authenticationManager.TryAuthenticateAsync(context.Request, routeConfig);
        if (!isAuthenticated)
        {
            logger.LogWarning("Unauthorized request to: {UpstreamRoute} [Activity ID: {ActivityId}]", routeConfig.Route.Upstream, activity!.Id);
            context.Response.StatusCode = 401;
            return false;
        }

        if (authorizationManager.IsAuthorized(context.User, routeConfig))
        {
            return true;
        }

        logger.LogWarning("Forbidden request to: {UpstreamRoute} by user: {UserName} [Activity ID: {ActivityId}]",
            routeConfig.Route.Upstream, context.User?.Identity?.Name, activity!.Id);
        context.Response.StatusCode = 403;

        return false;
    }
}