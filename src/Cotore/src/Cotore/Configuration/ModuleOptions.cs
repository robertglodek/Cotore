namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class ModuleOptions
{
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public bool? Enabled { get; init; }
    public List<RouteOptions> Routes { get; init; } = [];
    public Dictionary<string, ServiceOptions> Services { get; init; } = [];
}

