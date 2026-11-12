namespace Fargo.Core.Common;

public enum FargoErrorType
{
    None = 0,

    InvalidOperation = 1,

    EntityNotFound = 2,

    PermissionDenied = 3,

    AccessDenied = 4,

    InvalidCredentials = 5,

    NotAuthorized = 6,

    /// <summary>
    /// Optimistic concurrency failure: the resource was modified concurrently.
    /// </summary>
    Conflict = 7,
}
