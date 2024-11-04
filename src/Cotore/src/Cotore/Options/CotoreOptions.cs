using Cotore.Configuration;

namespace Cotore.Options;

public sealed class CotoreOptions
{
    public bool UseForwardedHeaders { get; set; }
    public bool? ForwardRequestHeaders { get; set; }
    public bool? ForwardResponseHeaders { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; } = [];
    public Dictionary<string, string> ResponseHeaders { get; set; } = [];
    public bool? ForwardStatusCode { get; set; }
    public bool? PassQueryString { get; set; }
    public AuthOptions? Auth { get; set; }
    public string? ModulesPath { get; set; }
    public string? PayloadsFolder { get; set; }
    public Dictionary<string, ModuleOptions> Modules { get; set; } = [];
    public HttpOptions Http { get; set; } = new();
    public LoadBalancerOptions? LoadBalancer { get; set; }
    public bool UseLocalUrl { get; set; }
}