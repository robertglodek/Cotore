using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Cotore.Options;
using Microsoft.Extensions.Options;

namespace Cotore.Auth;

internal sealed class AuthenticationManager(IOptions<CotoreOptions> options) : IAuthenticationManager
{
    private readonly CotoreOptions _options = options.Value;

    public async Task<bool> TryAuthenticateAsync(HttpRequest request, RouteConfig routeConfig)
    {
        if (!IsAuthenticationRequired(routeConfig))
        {
            return true;
        }

        var result = await request.HttpContext.AuthenticateAsync();

        return result.Succeeded;
    }

    private bool IsAuthenticationRequired(RouteConfig routeConfig)
        => (_options.Auth?.Enabled == true && routeConfig.Route?.Auth != false) || (routeConfig.Route?.Auth == true);
}