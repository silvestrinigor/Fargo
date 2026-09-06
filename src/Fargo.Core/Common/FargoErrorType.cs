namespace Fargo.Core.Common;

/// <summary>
/// Defines the categories of errors that can be associated with a <see cref="FargoCoreException"/>.
/// </summary>
public enum FargoErrorType
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
    /// The entity was not found.
    /// </summary>
    EntityNotFound = 2,

    /// <summary>
    /// Permission was denied.
    /// </summary>
    PermissionDenied = 3,

    /// <summary>
    /// Access was denied.
    /// </summary>
    AccessDenied = 4,
}
