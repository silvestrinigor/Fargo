using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;

namespace Fargo.Application.Operations.Identity;

/// <summary>
/// Exception thrown when login fails due to invalid credentials.
/// </summary>
/// <remarks>
/// Used when the user cannot be authenticated — whether because the user does not exist,
/// the password is incorrect, or the account is inactive. A single exception type is
/// intentionally used for all these cases to avoid leaking information about which
/// condition was triggered.
/// </remarks>
public sealed class InvalidCredentialsFargoApplicationException : FargoApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidCredentialsFargoApplicationException"/> class.
    /// </summary>
    /// <param name="internalReason">
    /// The specific cause, written to the logs only; it is never returned to the client.
    /// </param>
    public InvalidCredentialsFargoApplicationException(string? internalReason = null)
        : base("The provided credentials are invalid.", FargoErrorType.InvalidCredentials)
    {
        InternalReason = internalReason;
    }
}
