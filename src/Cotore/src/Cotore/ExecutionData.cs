using System.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cotore;

public sealed class ExecutionData
{
    public string? UserId { get; set; }
    public IDictionary<string, string> Claims { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public Configuration.RouteOptions Route { get; set; } = null!;
    public HttpContext Context { get; set; } = null!;
    public RouteData Data { get; set; } = null!;
    public string? Downstream { get; set; }
    public ExpandoObject? Payload { get; set; }
    public bool HasPayload { get; set; }
    public IEnumerable<Error> ValidationErrors { get; set; } = [];
    public bool IsPayloadValid => ValidationErrors is null || !ValidationErrors.Any();
}