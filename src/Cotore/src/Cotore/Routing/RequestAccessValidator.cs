using Cotore.Auth;
using System.Diagnostics;
using Cotore.Options;

namespace Cotore.Routing;

internal sealed class RequestAccessValidator(
    IAuthenticationManager authenticationManager,
    IAuthorizationManager authorizationManager,
    ILogger<RequestAccessValidator> logger,
    IOptions<CotoreOptions> options) : IRequestAccessValidator
{
    public async Task<bool> ValidateAsync(HttpContext context, RouteConfig routeConfig)
    {
        if (ShouldSkipAuth(routeConfig))
        {
            return true;
        }

        var activity = Activity.Current;

        if (!await IsAuthenticatedAsync(context, routeConfig, activity))
        {
            context.Response.StatusCode = 401;
            return false;
        }

        if (IsAuthorized(context, routeConfig, activity))
        {
            return true;
        }

        context.Response.StatusCode = 403;
        return false;
    }

    private bool ShouldSkipAuth(RouteConfig routeConfig) =>
        options.Value.Auth?.Enabled != true && routeConfig.Route.Auth != true;

    private async Task<bool> IsAuthenticatedAsync(HttpContext context, RouteConfig routeConfig, Activity? activity)
    {
        var isAuthenticated = await authenticationManager.IsAuthenticated(context.Request, routeConfig);
        if (!isAuthenticated)
        {
            logger.LogWarning("Unauthorized request to: {UpstreamRoute} [Activity ID: {ActivityId}]",
                routeConfig.Upstream, activity!.Id);
        }
        return isAuthenticated;
    }

    private bool IsAuthorized(HttpContext context, RouteConfig routeConfig, Activity? activity)
    {
        if (authorizationManager.IsAuthorized(context.User, routeConfig))
        {
            return true;
        }

        logger.LogWarning("Forbidden request to: {UpstreamRoute} by user: {UserName} [Activity ID: {ActivityId}]",
            routeConfig.Upstream, context.User.Identity?.Name, activity!.Id);

        return false;
    }
}