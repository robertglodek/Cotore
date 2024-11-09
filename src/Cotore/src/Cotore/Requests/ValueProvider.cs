namespace Cotore.Requests;

internal sealed class ValueProvider : IValueProvider
{
    private static readonly string[] AvailableTokens = ["user_id"];

    public IEnumerable<string> Tokens => AvailableTokens;

    public string? Get(string value, HttpRequest request, RouteData data)
        => $"{value?.ToLowerInvariant()}" switch
        {
            "@user_id" => request.HttpContext?.User?.Identity?.Name,
            _ => value,
        };
}