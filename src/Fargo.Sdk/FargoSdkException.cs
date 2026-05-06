namespace Fargo.Sdk;

/// <summary>
/// Serves as the base class for all exceptions thrown by the Fargo SDK.
/// </summary>
public abstract class FargoSdkException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="FargoSdkException"/> with no message.
    /// </summary>
    protected FargoSdkException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FargoSdkException"/> with the specified message.
    /// </summary>
    /// <param name="message">A description of the error.</param>
    protected FargoSdkException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FargoSdkException"/> with the specified message and inner exception.
    /// </summary>
    /// <param name="message">A description of the error.</param>
    /// <param name="innerException">The exception that caused this exception, or <see langword="null"/>.</param>
    protected FargoSdkException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
