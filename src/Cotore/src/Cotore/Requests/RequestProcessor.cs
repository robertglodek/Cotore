using Cotore.Routing;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;

namespace Cotore.Requests;

internal sealed class RequestProcessor(
    IPayloadTransformer payloadTransformer,
    IPayloadBuilder payloadBuilder,
    IPayloadValidator payloadValidator,
    IDownstreamBuilder downstreamBuilder) : IRequestProcessor
{
    private static readonly string[] SkipPayloadMethods = ["get", "delete", "head", "options", "trace"];

    public async Task<ExecutionData> ProcessAsync(RouteConfig routeConfig, HttpContext context)
    {
        context.Request.Headers.TryGetValue(HeaderNames.ContentType, out var contentType);
        var contentTypeValue = contentType.ToString().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(contentTypeValue) || contentTypeValue.Contains(MediaTypeNames.Text.Plain))
        {
            contentTypeValue = MediaTypeNames.Application.Json;
        }

        var route = routeConfig.Route;
        var skipPayload = route.Use == "downstream" && SkipPayloadMethods.Contains(route.DownstreamMethod);
        var routeData = context.GetRouteData();
        var hasTransformations = !skipPayload && payloadTransformer.HasTransformations(route);
        var payload = hasTransformations
            ? payloadTransformer.Transform(await payloadBuilder.BuildRawAsync(context.Request), route, context.Request, routeData)
            : null;

        var executionData = new ExecutionData
        {
            UserId = context.Request.HttpContext.User?.Identity?.Name,
            Claims = context.Request.HttpContext.User?.Claims?.ToDictionary(c => c.Type, c => c.Value) ?? [],
            ContentType = contentTypeValue,
            Route = routeConfig.Route,
            Context = context,
            Data = routeData,
            Downstream = downstreamBuilder.GetDownstream(routeConfig, context.Request, routeData),
            Payload = payload?.Payload,
            HasPayload = hasTransformations
        };

        if (skipPayload || payload is null)
        {
            return executionData;
        }

        executionData.ValidationErrors = await payloadValidator.GetValidationErrorsAsync(payload);

        return executionData;
    }
}