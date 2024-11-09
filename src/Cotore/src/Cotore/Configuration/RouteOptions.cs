namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class RouteOptions
{
    public string Upstream { get; set; } = null!;
    public string? Method { get; set; }
    public List<string> Methods { get; init; } = [];
    public bool MatchAll { get; init; }
    public string Use { get; init; } = string.Empty;
    public string? Downstream { get; init; }
    public string? DownstreamMethod { get; set; }
    public bool? PassQueryString { get; init; }
    public string? ReturnValue { get; init; }
    public string? Payload { get; init; }
    public string? Schema { get; init; }
    public bool? Auth { get; init; }
    public Dictionary<string, string> RequestHeaders { get; init; } = [];
    public Dictionary<string, string> ResponseHeaders { get; init; } = [];
    public bool? ForwardRequestHeaders { get; init; }
    public bool? ForwardResponseHeaders { get; init; }
    public bool? ForwardStatusCode { get; init; }
    public OnErrorOptions? OnError { get; init; }
    public OnSuccessOptions? OnSuccess { get; init; }
    public Dictionary<string, string> Claims { get; init; } = [];
    public List<string> Policies { get; init; } = [];
    public List<string> Bind { get; init; } = [];
    public List<string> Transform { get; init; } = [];
}