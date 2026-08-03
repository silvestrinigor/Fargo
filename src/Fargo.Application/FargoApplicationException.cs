namespace Fargo.Application;

/// <summary>
/// Represents the base exception for errors thrown by the Fargo application.
/// </summary>
public class FargoApplicationException : Exception
{
    private const string defaultExceptionMessage = "Fargo application exception.";

    /// <summary>
    /// Gets the error category associated with the exception.
    /// </summary>
    public FargoApplicationErrorType ErrorType { get; init; } = FargoApplicationErrorType.None;

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
    public FargoApplicationException(string? message, FargoApplicationErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error message, error type, and inner exception.
    /// </summary>
    public FargoApplicationException(string? message, FargoApplicationErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoApplicationException"/> class with a specified error type and inner exception.
    /// </summary>
    public FargoApplicationException(FargoApplicationErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
