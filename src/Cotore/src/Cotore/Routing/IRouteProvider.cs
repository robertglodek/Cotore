using Microsoft.AspNetCore.Routing;

namespace Cotore.Routing;

internal interface IRouteProvider
{
    Action<IEndpointRouteBuilder> Build();
}