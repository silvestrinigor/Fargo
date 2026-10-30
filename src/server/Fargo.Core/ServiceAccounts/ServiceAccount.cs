using Fargo.Core.Actors;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Security;

namespace Fargo.Core.ServiceAccounts;

/// <summary>
/// A non-interactive machine identity that authenticates through the
/// client-credentials flow (client id + secret) rather than as a human user.
/// </summary>
/// <remarks>
/// A service account is a first-class <see cref="Actor"/> (resolved as
/// <see cref="ActorType.Service"/>), carrying its own permissions and partition
/// accesses exactly like a user, so the existing authorization and audit
/// machinery apply unchanged. The client secret is never stored in clear: only
/// its hash is persisted. Rotating the secret bumps <see cref="AuthVersion"/>,
/// which invalidates any tokens previously issued to the account.
/// </remarks>
public sealed class ServiceAccount : IEntity, IEntityTyped, IConcurrencyVersioned
{
    /// <summary>Gets the unique identifier of the service account.</summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>Gets the unique, human-readable client identifier.</summary>
    public Nameid ClientId { get; private init; }

    /// <summary>Gets the hash of the client secret used to authenticate the account.</summary>
    /// <remarks>Intended for verification and persistence only; never exposed.</remarks>
    public PasswordHash SecretHash { get; private set; }

    /// <summary>Gets an optional human-readable description.</summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>Gets a value indicating whether the account can authenticate.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets the token-invalidation version. Any change (secret rotation,
    /// deactivation) bumps it so previously issued tokens stop validating.
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

    /// <summary>Gets the permissions granted directly to the service account.</summary>
    public IReadOnlyCollection<ActionType> Permissions => permissions;

    private readonly List<ActionType> permissions = [];

    /// <summary>Gets the identifiers of the partitions the account may access.</summary>
    public IReadOnlyCollection<Guid> PartitionAccessGuids => partitionAccessGuids;

    private readonly List<Guid> partitionAccessGuids = [];

    private ServiceAccount() { }

    private ServiceAccount(Nameid clientId, PasswordHash secretHash)
    {
        ClientId = clientId;
        SecretHash = secretHash;
    }

    /// <summary>Creates a new service account with the given client id and hashed secret.</summary>
    public static ServiceAccount Create(Nameid clientId, PasswordHash secretHash)
        => new(clientId, secretHash);

    /// <inheritdoc/>
    public EntityType GetEntityType() => EntityType.ServiceAccount;

    /// <summary>Replaces the stored secret hash and invalidates existing tokens.</summary>
    public void RotateSecret(PasswordHash newSecretHash)
    {
        SecretHash = newSecretHash;
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>Activates the account.</summary>
    public void Activate() => IsActive = true;

    /// <summary>Deactivates the account and invalidates its existing tokens.</summary>
    public void Deactivate()
    {
        IsActive = false;
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>Grants a permission. Idempotent.</summary>
    public void AddPermission(ActionType permission)
    {
        if (!permissions.Contains(permission))
        {
            permissions.Add(permission);
        }
    }

    /// <summary>Revokes a permission if present.</summary>
    public void RemovePermission(ActionType permission) => permissions.Remove(permission);

    /// <summary>Grants access to a partition. Idempotent.</summary>
    public void AddPartitionAccess(Guid partitionGuid)
    {
        if (!partitionAccessGuids.Contains(partitionGuid))
        {
            partitionAccessGuids.Add(partitionGuid);
        }
    }

    /// <summary>Revokes access to a partition if present.</summary>
    public void RemovePartitionAccess(Guid partitionGuid) => partitionAccessGuids.Remove(partitionGuid);
}
