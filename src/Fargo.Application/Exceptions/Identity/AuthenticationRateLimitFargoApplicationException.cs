using Fargo.Application.Common.Exceptions;
using Fargo.Core.Common;

namespace Fargo.Application.Exceptions.Identity;

public class AuthenticationRateLimitFargoApplicationException : FargoApplicationException
{
    public TimeSpan RetryAfter { get; }

    /// <param name="internalReason">Written to the logs only, for example who was limited.</param>
    public AuthenticationRateLimitFargoApplicationException(TimeSpan retryAfter, string? internalReason = null) : base(
        message: $"Too many authentication attempts. Please try again in {retryAfter}.", errorType: FargoErrorType.NotAuthorized)
    {
        RetryAfter = retryAfter;
        InternalReason = internalReason;
    }
}
