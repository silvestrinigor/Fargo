using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;

namespace Fargo.Application.Exceptions.Identity;

/// <remarks>
/// One type for an unknown user, a wrong password and an inactive account, so the response does not
/// reveal which one it was.
/// </remarks>
public sealed class InvalidCredentialsFargoApplicationException : FargoApplicationException
{
    /// <param name="internalReason">
    /// The specific cause. Written to the logs only, never returned to the client.
    /// </param>
    public InvalidCredentialsFargoApplicationException(string? internalReason = null)
        : base("The provided credentials are invalid.", FargoErrorType.InvalidCredentials)
    {
        InternalReason = internalReason;
    }
}
