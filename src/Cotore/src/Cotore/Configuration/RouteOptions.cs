namespace Cotore.Configuration;

public sealed class RouteOptions
{
    public string Upstream { get; set; } = null!;
    public string? Method { get; set; }
    public List<string> Methods { get; set; } = [];
    public bool MatchAll { get; set; }
    public string Use { get; set; } = string.Empty;
    public string? Downstream { get; set; }
    public string? DownstreamMethod { get; set; }
    public bool? PassQueryString { get; set; }
    public string? ReturnValue { get; set; }
    public string? Payload { get; set; }
    public string? Schema { get; set; }
    public bool? Auth { get; set; }
    public Dictionary<string, string> RequestHeaders { get; set; } = [];
    public Dictionary<string, string> ResponseHeaders { get; set; } = [];
    public bool? ForwardRequestHeaders { get; set; }
    public bool? ForwardResponseHeaders { get; set; }
    public bool? ForwardStatusCode { get; set; }
    public OnErrorOptions? OnError { get; set; }
    public OnSuccessOptions? OnSuccess { get; set; }
    public Dictionary<string, string> Claims { get; set; } = [];
    public List<string> Policies { get; set; } = [];
    public List<string> Bind { get; set; } = [];
    public List<string> Transform { get; set; } = [];
}