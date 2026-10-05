using Fargo.Core.Entities;

namespace Fargo.Core.Identity;

public sealed class AuthenticationAttempt : IEntity
{
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public string ActorIdentifier { get; private init; } = null!;

    public string IpAddress { get; private init; } = null!;

    public DateTimeOffset OccuredAt { get; private init; } = DateTimeOffset.UtcNow;

    public bool Successful { get; private init; }

    private AuthenticationAttempt() { }

    public AuthenticationAttempt(string actorIdentifier, string ipAddress, bool successful)
    {
        ActorIdentifier = actorIdentifier;

        IpAddress = ipAddress;

        Successful = successful;
    }
}
