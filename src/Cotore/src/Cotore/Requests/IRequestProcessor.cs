using Microsoft.AspNetCore.Http;

namespace Cotore.Requests;

public interface IRequestProcessor
{
    Task<ExecutionData> ProcessAsync(RouteConfig routeConfig, HttpContext context);
}