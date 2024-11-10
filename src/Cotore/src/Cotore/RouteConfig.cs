using RouteOptions = Cotore.Configuration.RouteOptions;

namespace Cotore;

public sealed class RouteConfig
{
    public RouteOptions Route { get; set; } = null!;
    public string? Downstream { get; set; }
    public string Upstream { get; set; } = null!;
}