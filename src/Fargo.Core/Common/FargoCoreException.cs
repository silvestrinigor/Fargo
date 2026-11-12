namespace Fargo.Core.Common;

public class FargoCoreException : Exception
{
    private const string defaultExceptionMessage = "Fargo core exception.";

    public FargoErrorType ErrorType { get; init; } = FargoErrorType.None;

    public FargoCoreException() : base(defaultExceptionMessage) { }

    public FargoCoreException(string? message) : base(message) { }

    public FargoCoreException(string? message, Exception innerException)
        : base(message, innerException) { }

    public FargoCoreException(string? message, FargoErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    public FargoCoreException(string? message, FargoErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    public FargoCoreException(FargoErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
