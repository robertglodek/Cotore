namespace Cotore.Configuration;

[UsedImplicitly]
public sealed class OnSuccessOptions
{
    public int Code { get; init; } = 200;
    public object? Data { get; init; }
}