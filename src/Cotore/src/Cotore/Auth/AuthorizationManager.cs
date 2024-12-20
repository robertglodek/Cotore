using System.Security.Claims;

namespace Cotore.Auth;

internal sealed class AuthorizationManager(IPolicyManager policyManager) : IAuthorizationManager
{
    public bool IsAuthorized(ClaimsPrincipal? user, RouteConfig routeConfig)
    {
        if (user == null)
        {
            return false;
        }

        return HasRequiredPolicies(user, routeConfig.Route.Policies) &&
               HasRequiredClaims(user, routeConfig.Route.Claims);
    }

    private bool HasRequiredPolicies(ClaimsPrincipal user, IEnumerable<string> policies)
        => policies.All(policy => HasPolicy(user, policy));

    private bool HasPolicy(ClaimsPrincipal user, string policy)
    {
        var requiredClaims = policyManager.GetClaims(policy);
        return HasRequiredClaims(user, requiredClaims);
    }

    private static bool HasRequiredClaims(ClaimsPrincipal user, IDictionary<string, string>? claims)
        => claims == null || claims.All(claim => user.HasClaim(claim.Key, claim.Value));
}