namespace Fargo.Application;

/// <summary>
/// Represents the base exception type for errors that occur
/// in the application layer.
/// </summary>
/// <remarks>
/// Application exceptions represent failures related to
/// application use cases, orchestration, or external inputs,
/// but not domain rule violations.
/// </remarks>
public class FargoApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="FargoApplicationException"/>.
    /// </summary>
    public FargoApplicationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FargoApplicationException"/>
    /// with a specified error message.
    /// </summary>
    public FargoApplicationException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FargoApplicationException"/>
    /// with a specified error message and inner exception.
    /// </summary>
    public FargoApplicationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
