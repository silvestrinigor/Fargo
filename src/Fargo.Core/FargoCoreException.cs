namespace Fargo.Core;

/// <summary>
/// 
/// </summary>
public class FargoCoreException : Exception
{
    private const string defaultExceptionMessage = "Fargo exception.";

    /// <summary>
    /// 
    /// </summary>
    public FargoCoreErrorType ErrorType { get; init; } = FargoCoreErrorType.None;

    /// <summary>
    /// 
    /// </summary>
    public FargoCoreException() : base(defaultExceptionMessage) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public FargoCoreException(string? message) : base(message) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoCoreException(string? message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorType"></param>
    public FargoCoreException(string? message, FargoCoreErrorType errorType)
        : base(message)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoCoreException(string? message, FargoCoreErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoCoreException(FargoCoreErrorType errorType, Exception innerException)
        : base(defaultExceptionMessage, innerException)
    {
        ErrorType = errorType;
    }
}
