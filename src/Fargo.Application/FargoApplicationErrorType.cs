namespace Fargo.Application;

/// <summary>
/// Defines the categories of errors that can be associated with a <see cref="FargoApplicationErrorType"/>.
/// </summary>
public enum FargoApplicationErrorType
{
    /// <summary>
    /// No specific error category.
    /// </summary>
    None = 0,

    /// <summary>
    /// 
    /// </summary>
    AccessDenied = 1,

    /// <summary>
    /// 
    /// </summary>
    PermissionDenied = 2,

    /// <summary>
    /// 
    /// </summary>
    EntityNotFound = 3,

    /// <summary>
    /// 
    /// </summary>
    ActorNotFound = 4,
}
