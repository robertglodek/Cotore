namespace Cotore;

public sealed class RouteConfig
{
    public Configuration.RouteOptions Route { get; set; } = null!;
    public string? Downstream { get; set; }
}