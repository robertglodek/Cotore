namespace Cotore.Exceptions;

public sealed class ConfigurationException(string message, string propertyPath) : CustomException(message)
{
    public string PropertyPath { get; } = propertyPath;
    public override string ToString() => $"{base.ToString()}, PropertyPath: {PropertyPath.ToLower()}";
}
