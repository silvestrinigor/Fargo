using Fargo.Core.Common;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;

namespace Fargo.Core.Entities.Actors;

/// <summary>
/// An identity that can authenticate and perform operations: either a human
/// <see cref="Users.User"/> or a machine <see cref="ServiceAccounts.ServiceAccount"/>.
/// </summary>
/// <remarks>
/// The kind-specific data lives in the owned <see cref="User"/> / <see cref="ServiceAccount"/>
/// satellite. The authorization state shared by every kind (permissions, partition membership
/// and direct partition accesses) lives on the actor itself.
/// </remarks>
public class Actor : Entity, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    public ActorType ActorType { get; private init; } = ActorType.User;

    /// <summary>
    /// Username for a user actor, client identifier for a service account. Unique across all actors.
    /// </summary>
    public Nameid Nameid { get; set; }

    public bool IsActive { get; private set; } = true;

    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// <see langword="null"/> unless this actor is a user.
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// <see langword="null"/> unless this actor is a service account.
    /// </summary>
    public ServiceAccount? ServiceAccount { get; private set; }

    private bool IsAdminActor => Guid == FargoCoreWellKnowGuids.AdminUserGuid;

    /// <summary>
    /// Permissions granted directly to the actor; those inherited from user groups are not included.
    /// </summary>
    public IReadOnlyCollection<ActionType> Permissions => permissions;

    private readonly List<ActionType> permissions = [];

    /// <summary>
    /// Partitions the actor has been granted direct access to. Access inherited through the
    /// partition hierarchy is not represented here.
    /// </summary>
    public IReadOnlyCollection<ActorPartitionAccess> PartitionAccesses => partitionAccesses;

    private readonly List<ActorPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Partitions the actor belongs to (membership), as opposed to <see cref="PartitionAccesses"/>.
    /// </summary>
    public IReadOnlyCollection<ActorPartition> Partitions => partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ActorPartition> partitions = [];

    // Required by EF Core.
    private Actor()
    {
        EntityType = EntityType.Actor;
    }

    private Actor(ActorType actorType, Guid guid, Nameid nameid)
    {
        EntityType = EntityType.Actor;
        ActorType = actorType;
        Guid = guid;
        Nameid = nameid;
    }

    /// <summary>
    /// The user is always a member of the global partition; that membership cannot be removed.
    /// </summary>
    public static Actor NewUser(Nameid nameid)
    {
        var actor = new Actor(ActorType.User, Guid.NewGuid(), nameid);
        actor.partitions.Add(new ActorPartition(actor, FargoCoreWellKnowGuids.GlobalPartitionGuid));

        actor.User = User.CreateUser(actor);

        return actor;
    }

    /// <summary>
    /// Creates the built-in administrator, a member of the global partition with explicit access
    /// to it and a member of the administrators user group. These cannot be revoked.
    /// </summary>
    public static Actor NewAdministratorUser(Nameid nameid)
    {
        var actor = new Actor(ActorType.User, FargoCoreWellKnowGuids.AdminUserGuid, nameid);
        actor.partitions.Add(new ActorPartition(actor, FargoCoreWellKnowGuids.GlobalPartitionGuid));

        actor.User = User.CreateAdministratorUser(actor);

        actor.AddPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid);

        return actor;
    }

    /// <summary>
    /// Unlike a user, a service account has no mandatory global-partition membership.
    /// </summary>
    public static Actor NewServiceAccount(Nameid clientId, PasswordHash secretHash)
    {
        var actor = new Actor(ActorType.Service, Guid.NewGuid(), clientId);

        actor.ServiceAccount = ServiceAccount.Create(actor, secretHash);

        return actor;
    }

    public void Activate() => IsActive = true;

    /// <exception cref="FargoCoreException">The actor is the built-in administrator.</exception>
    public void Deactivate()
    {
        if (IsAdminActor)
        {
            throw new FargoCoreException(
                $"Cannot deactivate the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        IsActive = false;
    }

    /// <summary>
    /// Does nothing if the actor already has the permission.
    /// </summary>
    public void AddPermission(ActionType action)
    {
        if (!permissions.Contains(action))
        {
            permissions.Add(action);
        }
    }

    /// <exception cref="FargoCoreException">The actor is the built-in administrator.</exception>
    public void RemovePermission(ActionType action)
    {
        if (IsAdminActor)
        {
            throw new FargoCoreException(
                $"Cannot revoke permission '{action}' from the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        permissions.RemoveAll(x => x == action);
    }

    /// <summary>
    /// Does nothing if the actor is already a member of the partition.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The actor is the administrator and the partition is not the global partition.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (IsAdminActor && !partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"Cannot associate the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}' with the non-global partition '{partition.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new ActorPartition(this, partition));
    }

    /// <summary>
    /// Does nothing if the actor is not a member of the partition.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The partition is the global partition and the actor is a user (it is mandatory for users).
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (ActorType == ActorType.User && partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' is mandatory and cannot be removed from the actor '{Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(new ActorPartitionAccess(this, partition));
    }

    // Avoids loading the partition when only a well-known identifier is needed.
    internal void AddPartitionAccess(Guid partitionGuid)
    {
        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        if (partitionAccesses.Any(x => x.PartitionGuid == partitionGuid))
        {
            return;
        }

        partitionAccesses.Add(new ActorPartitionAccess(this, partitionGuid));
    }

    /// <exception cref="FargoCoreException">
    /// The actor is the administrator and the partition is the global partition.
    /// </exception>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        if (IsAdminActor && partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"Cannot revoke the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}' access to the global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitionAccesses.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <exception cref="FargoCoreException">The actor is the built-in administrator.</exception>
    public void ValidateCanBeDeleted()
    {
        if (IsAdminActor)
        {
            throw new FargoCoreException(
                $"The admin user '{Guid}' cannot be deleted.",
                FargoErrorType.InvalidOperation);
        }
    }
}
