using System.Security.Claims;

namespace Cotore.Auth;

internal sealed class AuthorizationManager(IPolicyManager policyManager) : IAuthorizationManager
{
    private readonly IPolicyManager _policyManager = policyManager;

    public bool IsAuthorized(ClaimsPrincipal user, RouteConfig routeConfig)
    {
        if (user == null)
        {
            return false;
        }

        if (routeConfig?.Route == null)
        {
            return true;
        }

        return HasRequiredPolicies(user, routeConfig.Route.Policies) &&
               HasRequiredClaims(user, routeConfig.Route.Claims);
    }

    private bool HasRequiredPolicies(ClaimsPrincipal user, IEnumerable<string> policies)
        => policies == null || policies.All(policy => HasPolicy(user, policy));

    private bool HasPolicy(ClaimsPrincipal user, string policy)
    {
        var requiredClaims = _policyManager.GetClaims(policy);
        return HasRequiredClaims(user, requiredClaims);
    }

    private static bool HasRequiredClaims(ClaimsPrincipal user, IDictionary<string, string>? claims)
        => claims == null || claims.All(claim => user.HasClaim(claim.Key, claim.Value));
}