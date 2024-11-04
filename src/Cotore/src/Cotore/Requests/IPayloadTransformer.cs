using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cotore.Requests;

internal interface IPayloadTransformer
{
    bool HasTransformations(Configuration.RouteOptions route);
    PayloadSchema Transform(string payload, Configuration.RouteOptions route, HttpRequest request, RouteData data);
}