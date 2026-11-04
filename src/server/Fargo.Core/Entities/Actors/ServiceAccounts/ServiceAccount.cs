using Fargo.Core.Identity.Credentials;

namespace Fargo.Core.Entities.Actors.ServiceAccounts;

/// <summary>
/// Represents the service-account-specific data of a service-account
/// <see cref="Actor"/>: a non-interactive machine identity that authenticates
/// through the client-credentials flow (client id + secret) rather than as a
/// human user.
/// </summary>
/// <remarks>
/// A service account is an owned satellite of an <see cref="Actors.Actor"/>
/// (keyed by the owning actor's identifier), in the same way an
/// <see cref="Articles.ArticlePack"/> is an owned satellite of an
/// <see cref="Articles.Article"/>. The authorization state shared by every actor
/// (permissions, partition membership and partition accesses) lives on the owning
/// <see cref="Actors.Actor"/>; this type carries only the fields unique to a
/// machine identity.
///
/// The client secret is never stored in clear: only its hash is persisted.
/// Rotating the secret bumps <see cref="AuthVersion"/>, which invalidates any
/// tokens previously issued to the account. Unlike a user, a service account has
/// no mandatory global-partition membership.
/// </remarks>
public sealed class ServiceAccount
{
    /// <summary>
    /// Gets the unique identifier of the owning actor (and therefore of the
    /// service account).
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the actor that owns this service-account data.
    /// </summary>
    public Actor Actor { get; private init; } = null!;

    /// <summary>Gets the hash of the client secret used to authenticate the account.</summary>
    /// <remarks>Intended for verification and persistence only; never exposed.</remarks>
    public PasswordHash SecretHash { get; private set; }

    /// <summary>
    /// Gets the token-invalidation version. Any change (secret rotation,
    /// deactivation) bumps it so previously issued tokens stop validating.
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

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

    /// <summary>
    /// Creates a new service account for the specified owning actor.
    /// </summary>
    /// <param name="actor">The owning actor.</param>
    /// <param name="secretHash">The hashed client secret.</param>
    internal static ServiceAccount Create(Actor actor, PasswordHash secretHash)
        => new(actor, secretHash);

    /// <summary>Replaces the stored secret hash and invalidates existing tokens.</summary>
    public void RotateSecret(PasswordHash newSecretHash)
    {
        SecretHash = newSecretHash;
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>
    /// Invalidates all previously issued tokens for the account.
    /// </summary>
    /// <remarks>
    /// Called when the account is deactivated so existing tokens stop validating.
    /// </remarks>
    public void RotateAuthVersion() => AuthVersion = Guid.NewGuid();
}
