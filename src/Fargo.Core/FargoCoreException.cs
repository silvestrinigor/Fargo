namespace Fargo.Core;

public class FargoCoreException : Exception
{
    private const string defaultExceptionMessage = "Fargo core exception.";

    public FargoCoreErrorType ErrorType { get; init; } = FargoCoreErrorType.None;

    public FargoCoreException() : base(defaultExceptionMessage) { }

    public FargoCoreException(string? message) : base(message) { }

    public FargoCoreException(string? message, Exception innerException)
        : base(message, innerException) { }

    public FargoCoreException(string? message, FargoCoreErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    public FargoCoreException(string? message, FargoCoreErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    public FargoCoreException(FargoCoreErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
