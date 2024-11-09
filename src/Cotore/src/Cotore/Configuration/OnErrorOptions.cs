namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class OnErrorOptions
{
    public int Code { get; init; } = 400;
    public object? Data { get; init; }
}