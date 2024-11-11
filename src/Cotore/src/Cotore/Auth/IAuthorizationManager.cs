using System.Security.Claims;

namespace Cotore.Auth;

public interface IAuthorizationManager
{
    bool IsAuthorized(ClaimsPrincipal? user, RouteConfig routeConfig);
}