namespace Cotore;

public class Error
{
    public string Code { get; set; } = null!;
    public string? Property { get; set; }
    public string Message { get; set; } = null!;
}