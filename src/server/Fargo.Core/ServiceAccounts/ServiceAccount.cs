using Fargo.Core.Actors;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Security;

namespace Fargo.Core.ServiceAccounts;

/// <summary>
/// A non-interactive machine identity that authenticates through the
/// client-credentials flow (client id + secret) rather than as a human user.
/// </summary>
/// <remarks>
/// A service account is a first-class <see cref="Actor"/> (resolved as
/// <see cref="ActorType.Service"/>), carrying its own permissions, partition
/// memberships and partition accesses exactly like a user, so the existing
/// authorization and audit machinery apply unchanged. The client secret is
/// never stored in clear: only its hash is persisted. Rotating the secret bumps
/// <see cref="AuthVersion"/>, which invalidates any tokens previously issued to
/// the account.
///
/// Like a user, a service account is a partitioned entity: its
/// <see cref="PartitionGuids"/> (partition membership) define the account's
/// partition scope and drive partition-based listing filters and audit-log
/// partition association. Unlike a user, a service account has no mandatory
/// global-partition membership: a service account is a scoped machine identity
/// whose memberships are assigned explicitly, so it is never implicitly a
/// member of the global partition.
/// </remarks>
public sealed class ServiceAccount : IEntity, IEntityTyped, IPartitionedGuidsReadOnly, IConcurrencyVersioned
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

    /// <summary>
    /// Gets the partitions to which the service account belongs (its partition
    /// membership).
    /// </summary>
    /// <remarks>
    /// Membership defines the account's partition scope and is distinct from
    /// <see cref="PartitionAccessGuids"/> (which governs what the account may
    /// reach). Unlike a user, a service account has no mandatory global-partition
    /// membership.
    /// </remarks>
    public IReadOnlyCollection<ServiceAccountPartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions to which the service
    /// account belongs.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ServiceAccountPartition> partitions = [];

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

    /// <summary>
    /// Associates the service account with the specified partition (partition
    /// membership). Idempotent.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new ServiceAccountPartition(this, partition));
    }

    /// <summary>
    /// Removes the association between the service account and the specified
    /// partition (partition membership). If the account is not associated with
    /// the partition, no action is taken.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the partition to remove.
    /// </param>
    public void RemovePartition(Guid partitionGuid)
        => partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
}
