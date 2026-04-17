namespace Application.Common.Exceptions;

public abstract class BusinessLogicException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
