namespace Cotore.Configuration;

public sealed class AuthOptions
{
    public bool Enabled { get; set; }
    public Dictionary<string, PolicyOptions> Policies { get; set; } = [];

    public sealed class PolicyOptions
    {
        public Dictionary<string, string> Claims { get; set; } = [];
    }
}