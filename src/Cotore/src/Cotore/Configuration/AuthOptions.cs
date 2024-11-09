namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class AuthOptions
{
    public bool Enabled { get; init; }
    public Dictionary<string, PolicyOptions> Policies { get; init; } = [];

    [UsedImplicitly]
    public sealed class PolicyOptions
    {
        public Dictionary<string, string> Claims { get; init; } = new();
    }
}