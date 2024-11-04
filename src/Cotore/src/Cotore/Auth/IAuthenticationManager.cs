using Microsoft.AspNetCore.Http;

namespace Cotore.Auth;

public interface IAuthenticationManager
{
    Task<bool> TryAuthenticateAsync(HttpRequest request, RouteConfig routeConfig);
}