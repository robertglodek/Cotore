using Microsoft.AspNetCore.Authentication;
using Cotore.Options;

namespace Cotore.Auth;

internal sealed class AuthenticationManager(IOptions<CotoreOptions> options) : IAuthenticationManager
{
    public async Task<bool> IsAuthenticated(HttpRequest request, RouteConfig routeConfig)
    {
        if (!IsAuthenticationRequired(routeConfig))
        {
            return true;
        }

        var result = await request.HttpContext.AuthenticateAsync();

        return result.Succeeded;
    }

    private bool IsAuthenticationRequired(RouteConfig routeConfig)
        => (options.Value.Auth?.Enabled == true && routeConfig.Route.Auth != false) || routeConfig.Route.Auth == true;
}