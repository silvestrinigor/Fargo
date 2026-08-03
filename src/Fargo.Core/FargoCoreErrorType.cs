namespace Fargo.Core;

/// <summary>
/// Defines the categories of errors that can be associated with a <see cref="FargoCoreException"/>.
/// </summary>
public enum FargoCoreErrorType
{
    /// <summary>
    /// No specific error category.
    /// </summary>
    None = 0,

    /// <summary>
    /// The operation is invalid.
    /// </summary>
    InvalidOperation = 1,

    /// <summary>
    /// The argument is invalid.
    /// </summary>
    InvalidArgument = 2,
}
