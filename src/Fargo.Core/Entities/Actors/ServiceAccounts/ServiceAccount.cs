using Fargo.Core.Identity.Credentials;

namespace Fargo.Core.Entities.Actors.ServiceAccounts;

/// <summary>
/// The machine-identity data of a service-account <see cref="Actor"/>, which authenticates through
/// the client-credentials flow (client id + secret).
/// </summary>
/// <remarks>
/// Owned satellite of the actor; shared authorization state lives on the <see cref="Actors.Actor"/>.
/// Only the secret hash is persisted. Rotating the secret bumps <see cref="AuthVersion"/>, which
/// invalidates previously issued tokens.
/// </remarks>
public sealed class ServiceAccount
{
    public Guid ActorGuid { get; private init; }

    public Actor Actor { get; private init; } = null!;

    // Verification and persistence only; never exposed.
    public PasswordHash SecretHash { get; private set; }

    /// <summary>
    /// Token-invalidation version: any change bumps it so previously issued tokens stop validating.
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

    // Required by EF Core.
    private ServiceAccount()
    {
    }

    private ServiceAccount(Actor actor, PasswordHash secretHash)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Actor = actor;
        ActorGuid = actor.Guid;
        SecretHash = secretHash;
    }

    internal static ServiceAccount Create(Actor actor, PasswordHash secretHash)
        => new(actor, secretHash);

    /// <summary>
    /// Also invalidates existing tokens.
    /// </summary>
    public void RotateSecret(PasswordHash newSecretHash)
    {
        SecretHash = newSecretHash;
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>
    /// Invalidates all previously issued tokens; used when the account is deactivated.
    /// </summary>
    public void RotateAuthVersion() => AuthVersion = Guid.NewGuid();
}
