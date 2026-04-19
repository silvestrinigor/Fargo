namespace Fargo.Application.Authentication;

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
