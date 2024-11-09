using System.Text;
using Cotore.Options;
using Cotore.Requests;

namespace Cotore.Routing;

internal sealed class DownstreamBuilder(IOptions<CotoreOptions> options, IValueProvider valueProvider) : IDownstreamBuilder
{
    private readonly CotoreOptions _options = options.Value;

    public string? GetDownstream(RouteConfig routeConfig, HttpRequest request, RouteData data)
    {
        if (string.IsNullOrWhiteSpace(routeConfig.Downstream))
        {
            return null;
        }

        var stringBuilder = new StringBuilder();
        var downstream = routeConfig.Downstream;
        stringBuilder.Append(downstream);
        if (downstream.Contains('@'))
        {
            foreach (var token in valueProvider.Tokens)
            {
                var tokenName = $"@{token}";
                stringBuilder.Replace(tokenName, valueProvider.Get(tokenName, request, data));
            }
        }

        foreach (var (key, value) in data.Values)
        {
            if (value is null)
            {
                continue;
            }

            if (key is "url")
            {
                stringBuilder.Append($"/{value}");
                continue;
            }
            
            stringBuilder.Replace($"{{{key}}}", value.ToString());
        }

        if (_options.PassQueryString == false || routeConfig.Route.PassQueryString == false)
        {
            return stringBuilder.ToString();
        }

        var queryString = request.QueryString.ToString();
        if (downstream.Contains('?') && !string.IsNullOrWhiteSpace(queryString))
        {
            queryString = $"&{queryString.Substring(1, queryString.Length - 1)}";
        }

        stringBuilder.Append(queryString);

        return stringBuilder.ToString();
    }
}