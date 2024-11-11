using Cotore.Options;

namespace Cotore.Auth;

internal sealed class PolicyManager : IPolicyManager
{
    private readonly Dictionary<string, Dictionary<string, string>> _policies;
    private readonly CotoreOptions _options;

    public PolicyManager(IOptions<CotoreOptions> options)
    {
        _options = options.Value;
        _policies = InitializePolicies();
        VerifyPolicies();
    }

    public IDictionary<string, string>? GetClaims(string policy) => _policies.GetValueOrDefault(policy);

    private Dictionary<string, Dictionary<string, string>> InitializePolicies() =>
        _options.Auth?.Policies.ToDictionary(
            policy => policy.Key,
            policy => policy.Value.Claims.ToDictionary(claim 
                => claim.Key, claim => claim.Value)) ?? [];

    private void VerifyPolicies()
    {
        var definedPolicies = (_options.Modules)
          .SelectMany(module => module.Value.Routes)
          .SelectMany(route => route.Policies)
          .Distinct();

        var missingPolicies = definedPolicies.Except(_policies.Keys).ToArray();
        if (missingPolicies.Length > 0)
        {
            throw new InvalidOperationException($"Missing policies: '{string.Join(", ", missingPolicies)}'");
        }
    }

}