namespace Cotore.Configuration;

public sealed class ModuleOptions
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool? Enabled { get; set; }
    public List<RouteOptions> Routes { get; set; } = [];
    public Dictionary<string, ServiceOptions> Services { get; set; } = [];
}

