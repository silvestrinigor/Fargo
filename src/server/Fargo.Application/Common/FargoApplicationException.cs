using Fargo.Core.Common;

namespace Fargo.Application.Common;

/// <summary>
/// Represents the base exception for errors thrown by the Fargo application.
/// </summary>
public class FargoApplicationException : Exception
{
    private const string defaultExceptionMessage = "Fargo application exception.";

    /// <summary>
    /// Gets the error category associated with the exception.
    /// </summary>
    public FargoErrorType ErrorType { get; init; } = FargoErrorType.None;

    /// <summary>
    /// Gets diagnostic detail that is written to the logs but never returned to
    /// the client. Use it for the specific cause of a deliberately generic
    /// failure (for example, why a login was rejected) so the public message
    /// does not leak it.
    /// </summary>
    public string? InternalReason { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class.
    /// </summary>
    public FargoApplicationException() : base(defaultExceptionMessage) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error message.
    /// </summary>
    public FargoApplicationException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error message and inner exception.
    /// </summary>
    public FargoApplicationException(string? message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error message and error type.
    /// </summary>
    public FargoApplicationException(string? message, FargoErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error message, error type, and inner exception.
    /// </summary>
    public FargoApplicationException(string? message, FargoErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error type and inner exception.
    /// </summary>
    public FargoApplicationException(FargoErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
