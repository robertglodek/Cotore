using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cotore.Requests;

internal interface IValueProvider
{
    IEnumerable<string> Tokens { get; }
    string? Get(string value, HttpRequest request, RouteData data);
}