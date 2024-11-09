namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class LoadBalancerOptions
{
    public bool Enabled { get; init; }
    public string Url { get; init; } = string.Empty;
}