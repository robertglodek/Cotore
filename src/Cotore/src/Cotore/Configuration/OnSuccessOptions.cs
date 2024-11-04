namespace Cotore.Configuration;

public sealed class OnSuccessOptions
{
    public int Code { get; set; } = 200;
    public object? Data { get; set; }
}