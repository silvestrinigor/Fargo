namespace Fargo.Domain.Exceptions;

/// <summary>
/// Base exception for all domain-related errors in the Fargo domain model.
///
/// Domain exceptions represent violations of business rules and should
/// be thrown only from the domain layer.
/// </summary>
public abstract class FargoDomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FargoDomainException"/> class.
    /// </summary>
    protected FargoDomainException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoDomainException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    protected FargoDomainException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoDomainException"/> class
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    /// <param name="innerException">The inner exception.</param>
    protected FargoDomainException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
