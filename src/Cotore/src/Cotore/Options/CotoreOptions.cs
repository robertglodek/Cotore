using Cotore.Configuration;

namespace Cotore.Options;

public sealed class CotoreOptions
{
    public bool UseForwardedHeaders { get; init; }
    public bool? ForwardRequestHeaders { get; init; }
    public bool? ForwardResponseHeaders { get; init; }
    public Dictionary<string, string> RequestHeaders { get; init; } = [];
    public Dictionary<string, string> ResponseHeaders { get; init; } = [];
    public bool? ForwardStatusCode { get; init; }
    public bool? PassQueryString { get; init; }
    public AuthOptions? Auth { get; init; }
    public string? ModulesPath { get; init; }
    public string? PayloadsFolder { get; set; }
    public Dictionary<string, ModuleOptions> Modules { get; init; } = [];
    public HttpOptions Http { get; init; } = new();
    public LoadBalancerOptions? LoadBalancer { get; init; }
    public bool UseLocalUrl { get; init; }
}