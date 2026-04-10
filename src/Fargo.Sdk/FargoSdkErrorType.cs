namespace Fargo.Sdk;

/// <summary>
/// Categorizes the type of failure returned by an SDK operation.
/// </summary>
public enum FargoSdkErrorType
{
    /// <summary>The error could not be mapped to a known category.</summary>
    Undefined,

    /// <summary>The request requires authentication but no valid session exists.</summary>
    UnauthorizedAccess,

    /// <summary>The provided credentials were invalid.</summary>
    InvalidCredentials,

    /// <summary>The user must change their password before proceeding.</summary>
    PasswordChangeRequired,

    /// <summary>The requested resource does not exist.</summary>
    NotFound,

    /// <summary>The request contained invalid or malformed data.</summary>
    InvalidInput,

    /// <summary>The operation conflicts with existing data (e.g. duplicate name identifier).</summary>
    Conflict,

    /// <summary>The current user does not have permission to perform the operation.</summary>
    Forbidden,
}
