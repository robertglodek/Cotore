namespace Cotore.Exceptions;

public abstract class CustomException : Exception
{
    protected CustomException(string message) : base(message) { }
    protected CustomException(string message, Exception? innerException) : base(message, innerException) { }
}
