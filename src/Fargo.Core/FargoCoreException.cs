namespace Fargo.Core;

/// <summary>
/// Represents the base exception for errors thrown by the Fargo core.
/// </summary>
public class FargoCoreException : Exception
{
    private const string defaultExceptionMessage = "Fargo core exception.";

    /// <summary>
    /// Gets the error category associated with the exception.
    /// </summary>
    public FargoCoreErrorType ErrorType { get; init; } = FargoCoreErrorType.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoCoreException"/> class.
    /// </summary>
    public FargoCoreException() : base(defaultExceptionMessage) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoCoreException"/> class with a specified error message.
    /// </summary>
    public FargoCoreException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoCoreException"/> class with a specified error message and inner exception.
    /// </summary>
    public FargoCoreException(string? message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoCoreException"/> class with a specified error message and error type.
    /// </summary>
    public FargoCoreException(string? message, FargoCoreErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoCoreException"/> class with a specified error message, error type, and inner exception.
    /// </summary>
    public FargoCoreException(string? message, FargoCoreErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoCoreException"/> class with a specified error type and inner exception.
    /// </summary>
    public FargoCoreException(FargoCoreErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
