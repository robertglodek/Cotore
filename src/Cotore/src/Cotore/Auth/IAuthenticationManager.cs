namespace Cotore.Auth;

public interface IAuthenticationManager
{
    Task<bool> IsAuthenticated(HttpRequest request, RouteConfig routeConfig);
}