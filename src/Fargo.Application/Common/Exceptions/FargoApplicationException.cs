using Fargo.Core.Common;

namespace Fargo.Application.Common.Exceptions;

public class FargoApplicationException : Exception
{
    private const string defaultExceptionMessage = "Fargo application exception.";

    public FargoErrorType ErrorType { get; init; } = FargoErrorType.None;

    /// <summary>
    /// Diagnostic detail written to the logs but never returned to the client, for the specific
    /// cause of a deliberately generic failure (for example, why a login was rejected).
    /// </summary>
    public string? InternalReason { get; init; }

    public FargoApplicationException() : base(defaultExceptionMessage) { }

    public FargoApplicationException(string? message) : base(message) { }

    public FargoApplicationException(string? message, Exception innerException)
        : base(message, innerException) { }

    public FargoApplicationException(string? message, FargoErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    public FargoApplicationException(string? message, FargoErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    public FargoApplicationException(FargoErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
