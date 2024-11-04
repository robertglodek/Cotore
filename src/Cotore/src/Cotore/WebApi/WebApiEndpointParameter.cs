namespace Cotore.WebApi;

public sealed class WebApiEndpointParameter
{
    public string In { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public object? Example { get; set; }
}