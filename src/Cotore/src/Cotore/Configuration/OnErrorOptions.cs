namespace Cotore.Configuration;

public sealed class OnErrorOptions
{
    public int Code { get; set; } = 400;
    public object? Data { get; set; }
}