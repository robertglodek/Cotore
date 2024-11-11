namespace Cotore.Serialization;

internal sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);
    public T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options);
    public T? Deserialize<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, _options);
    public object? Deserialize(string value, Type type) => JsonSerializer.Deserialize(value, type, _options);
}
