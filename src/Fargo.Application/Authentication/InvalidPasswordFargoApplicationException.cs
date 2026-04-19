namespace Fargo.Application.Authentication;

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
