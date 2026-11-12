namespace Fargo.Core.Identity.Authentication;

/// <summary>
/// A successful or failed authentication attempt, recorded for rate limiting.
/// </summary>
public sealed class AuthenticationAttempt
{
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public string ActorIdentifier { get; private init; } = null!;

    public string IpAddress { get; private init; } = null!;

    public DateTimeOffset OccuredAt { get; private init; }

    public bool Successful { get; private init; }

    // Required by EF Core.
    private AuthenticationAttempt() { }

    public AuthenticationAttempt(string actorIdentifier, string ipAddress, bool successful, DateTimeOffset occurredAt)
    {
        ActorIdentifier = actorIdentifier;

        IpAddress = ipAddress;

        Successful = successful;

        OccuredAt = occurredAt;
    }
}
