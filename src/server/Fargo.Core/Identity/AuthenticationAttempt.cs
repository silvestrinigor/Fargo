using Fargo.Core.Entities;

namespace Fargo.Core.Identity;

/// <summary>
/// Represents a single authentication attempt, either successful or failed.
/// </summary>
/// <remarks>
/// Authentication attempts are recorded to support rate limiting and to detect
/// suspicious activity such as repeated failed logins.
/// </remarks>
public sealed class AuthenticationAttempt : IEntity
{
    /// <summary>
    /// Gets the unique identifier of the authentication attempt.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the identifier of the actor that attempted to authenticate.
    /// </summary>
    public string ActorIdentifier { get; private init; } = null!;

    /// <summary>
    /// Gets the IP address from which the authentication attempt originated.
    /// </summary>
    public string IpAddress { get; private init; } = null!;

    /// <summary>
    /// Gets the date and time, in UTC, at which the attempt occurred.
    /// </summary>
    public DateTimeOffset OccuredAt { get; private init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the authentication attempt succeeded.
    /// </summary>
    public bool Successful { get; private init; }

    private AuthenticationAttempt() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationAttempt"/> class.
    /// </summary>
    /// <param name="actorIdentifier">
    /// The identifier of the actor that attempted to authenticate.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address from which the authentication attempt originated.
    /// </param>
    /// <param name="successful">
    /// A value indicating whether the authentication attempt succeeded.
    /// </param>
    public AuthenticationAttempt(string actorIdentifier, string ipAddress, bool successful)
    {
        ActorIdentifier = actorIdentifier;

        IpAddress = ipAddress;

        Successful = successful;
    }
}
