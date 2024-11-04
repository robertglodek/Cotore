using Cotore.Configuration;

namespace Cotore;

public sealed class RouteConfig
{
    public RouteOptions Route { get; set; } = null!;
    public string? Downstream { get; set; }
}