namespace Fargo.Application;

/// <summary>
/// 
/// </summary>
public class FargoApplicationException : Exception
{
    private const string defaultExceptionMessage = "Fargo exception.";

    /// <summary>
    /// 
    /// </summary>
    public FargoApplicationErrorType ErrorType { get; init; } = FargoApplicationErrorType.None;

    /// <summary>
    /// 
    /// </summary>
    public FargoApplicationException() : base(defaultExceptionMessage) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public FargoApplicationException(string? message) : base(message) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoApplicationException(string? message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorType"></param>
    public FargoApplicationException(string? message, FargoApplicationErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoApplicationException(string? message, FargoApplicationErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoApplicationException(FargoApplicationErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}