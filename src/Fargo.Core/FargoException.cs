namespace Fargo.Core;

/// <summary>
/// 
/// </summary>
public class FargoException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public FargoException(string? message) : base(message) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FargoException(string? message, Exception innerException)
        : base(message, innerException) { }
}
