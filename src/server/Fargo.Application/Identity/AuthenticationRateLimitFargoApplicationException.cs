using Fargo.Application.Common;
using Fargo.Core.Common;

namespace Fargo.Application.Identity;

/// <summary>
/// Represents an application exception thrown when authentication attempts exceed rate limits.
/// This exception indicates that the actor has made too many authentication attempts and must wait
/// before trying again, helping to prevent brute force attacks.
/// </summary>
public class AuthenticationRateLimitFargoApplicationException : FargoApplicationException
{
    /// <summary>
    /// Gets the time span after which the actor can retry authentication attempts.
    /// </summary>
    public TimeSpan RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the AuthenticationRateLimitFargoApplicationException class.
    /// </summary>
    /// <param name="retryAfter">The time span after which the actor can retry authentication attempts</param>
    public AuthenticationRateLimitFargoApplicationException(TimeSpan retryAfter) : base(
        message: $"Too many authentication attempts. Please try again in {retryAfter}.", errorType: FargoErrorType.NotAuthorized)
    {
        RetryAfter = retryAfter;
    }
}
