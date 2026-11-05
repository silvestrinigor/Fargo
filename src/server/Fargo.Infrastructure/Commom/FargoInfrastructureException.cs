using Fargo.Core.Common;

namespace Fargo.Infrastructure.Exceptions;

/// <summary>
/// Represents the base exception for errors thrown by the Fargo infrastructure.
/// </summary>
public class FargoInfrastructureException : Exception
{
    private const string defaultExceptionMessage = "Fargo infrastructure exception.";

    /// <summary>
    /// Gets the error category associated with the exception.
    /// </summary>
    public FargoErrorType ErrorType { get; init; } = FargoErrorType.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoInfrastructureException"/> class.
    /// </summary>
    public FargoInfrastructureException() : base(defaultExceptionMessage) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoInfrastructureException"/> class with a specified error message.
    /// </summary>
    public FargoInfrastructureException(string? message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoInfrastructureException"/> class with a specified error message and inner exception.
    /// </summary>
    public FargoInfrastructureException(string? message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoInfrastructureException"/> class with a specified error message and error type.
    /// </summary>
    public FargoInfrastructureException(string? message, FargoErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoInfrastructureException"/> class with a specified error message, error type, and inner exception.
    /// </summary>
    public FargoInfrastructureException(string? message, FargoErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoInfrastructureException"/> class with a specified error type and inner exception.
    /// </summary>
    public FargoInfrastructureException(FargoErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
