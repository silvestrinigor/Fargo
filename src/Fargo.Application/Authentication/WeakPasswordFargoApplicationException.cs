namespace Fargo.Application.Exceptions;

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
