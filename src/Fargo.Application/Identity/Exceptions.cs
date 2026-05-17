namespace Fargo.Application.Identity;

/// <summary>
/// Exception thrown when login fails due to invalid credentials.
/// </summary>
/// <remarks>
/// Used when the user cannot be authenticated — whether because the user does not exist,
/// the password is incorrect, or the account is inactive. A single exception type is
/// intentionally used for all these cases to avoid leaking information about which
/// condition was triggered.
/// </remarks>
public sealed class InvalidCredentialsFargoApplicationException()
    : FargoApplicationException("The provided credentials are invalid.");

/// <summary>
/// Exception thrown when a nameid string does not satisfy the required format rules.
/// </summary>
/// <param name="reason">A message describing the specific rule violation.</param>
public sealed class InvalidNameidFargoApplicationException(string reason)
    : FargoApplicationException(reason);

/// <summary>
/// Exception thrown when the provided password does not match
/// the current password of the user.
/// </summary>
/// <remarks>
/// This exception should only be used when the user is already authenticated
/// and their identity is known.
///
/// Returning a specific "invalid password" error for unauthenticated requests
/// can expose security information by allowing attackers to distinguish
/// between an invalid user identifier and an incorrect password.
///
/// For authentication failures where the user identity is not yet verified,
/// a generic authentication error (for example
/// <see cref="UnauthorizedAccessFargoApplicationException"/>) should be used instead.
/// </remarks>
public class InvalidPasswordFargoApplicationException()
    : FargoApplicationException("The provided password is incorrect.");

/// <summary>
/// Exception thrown when a user must change their password before accessing the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PasswordChangeRequiredFargoApplicationException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user who must change their password.
/// </param>
public sealed class PasswordChangeRequiredFargoApplicationException(Guid userGuid)
            : FargoApplicationException($"User '{userGuid}' must change their password before continuing.")
{
    /// <summary>
    /// Gets the identifier of the user who must change their password.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}

/// <summary>
/// Exception thrown when the current user is not authorized
/// to perform the requested operation.
/// </summary>
public class UnauthorizedAccessFargoApplicationException()
    : FargoApplicationException(
            "The current user is not authorized to perform this operation.");

/// <summary>
/// Exception thrown when a new password does not meet the required security policy.
/// </summary>
/// <remarks>
/// This exception is only thrown when setting or changing a password, never
/// during login or current-password verification.
/// </remarks>
/// <param name="reason">A message describing the specific policy violation.</param>
public sealed class WeakPasswordFargoApplicationException(string reason)
    : FargoApplicationException(reason);
