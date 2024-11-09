namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class ServiceOptions
{
    public string LocalUrl { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}