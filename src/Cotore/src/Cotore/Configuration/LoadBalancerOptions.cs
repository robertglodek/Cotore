namespace Cotore.Configuration;

public sealed class LoadBalancerOptions
{
    public bool Enabled { get; set; }
    public string Url { get; set; } = string.Empty;
}