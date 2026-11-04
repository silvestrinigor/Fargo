using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.ServiceAccounts;
using Fargo.Core.Users;

namespace Fargo.Core.Actors;

/// <summary>
/// Represents an actor: an identity that can authenticate and perform operations
/// within the system.
/// </summary>
/// <remarks>
/// An actor is the shared root for the different kinds of identity the system
/// recognises. Its concrete kind is given by <see cref="ActorType"/>: a human
/// <see cref="Users.User"/> (<see cref="ActorType.User"/>) or a non-interactive
/// machine <see cref="ServiceAccounts.ServiceAccount"/>
/// (<see cref="ActorType.Service"/>). The kind-specific data is held by the owned
/// <see cref="User"/> / <see cref="ServiceAccount"/> satellites, exactly as an
/// <see cref="Articles.Article"/> owns its variation or pack satellite.
///
/// The authorization state shared by every actor — its directly granted
/// <see cref="Permissions"/>, its partition <see cref="Partitions">membership</see>
/// and its direct <see cref="PartitionAccesses">partition accesses</see> — lives
/// on the actor itself rather than being duplicated across each kind.
/// </remarks>
public class Actor : IEntity, IEntityTyped, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the kind of actor (user or service account).
    /// </summary>
    public ActorType ActorType { get; private init; } = ActorType.User;

    /// <summary>
    /// Gets or sets the unique name identifier of the actor.
    /// </summary>
    /// <remarks>
    /// For a user actor this is its username; for a service-account actor this is
    /// its human-readable client identifier. It is unique across all actors.
    /// </remarks>
    public Nameid Nameid { get; set; }

    /// <summary>
    /// Gets a value indicating whether the actor is active and may authenticate.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets or sets the textual description associated with the actor.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the user-specific data when this actor is a user; otherwise
    /// <see langword="null"/>.
    /// </summary>
    public User? User { get; private set; }

    /// <summary>
    /// Gets the service-account-specific data when this actor is a service
    /// account; otherwise <see langword="null"/>.
    /// </summary>
    public ServiceAccount? ServiceAccount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this actor is the built-in administrator.
    /// </summary>
    /// <remarks>
    /// Only a user actor can be the administrator. This drives the actor-level
    /// guards that protect the built-in administrator.
    /// </remarks>
    private bool IsAdminActor => Guid == FargoCoreWellKnowGuids.AdminUserGuid;

    /// <summary>
    /// Gets the permissions granted directly to the actor.
    /// </summary>
    /// <remarks>
    /// Permissions inherited from user groups (for user actors) are not included.
    /// </remarks>
    public IReadOnlyCollection<ActionType> Permissions => permissions;

    private readonly List<ActionType> permissions = [];

    /// <summary>
    /// Gets the partitions to which the actor has been granted direct access.
    /// </summary>
    /// <remarks>
    /// Access to descendant partitions may be inherited through the partition
    /// hierarchy and is not represented by this collection.
    /// </remarks>
    public IReadOnlyCollection<ActorPartitionAccess> PartitionAccesses => partitionAccesses;

    private readonly List<ActorPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Gets the partitions to which the actor belongs (its partition membership).
    /// </summary>
    public IReadOnlyCollection<ActorPartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions to which the actor belongs.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ActorPartition> partitions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Actor"/> class.
    /// </summary>
    /// <remarks>Required by Entity Framework.</remarks>
    private Actor()
    {
    }

    private Actor(ActorType actorType, Guid guid, Nameid nameid)
    {
        ActorType = actorType;
        Guid = guid;
        Nameid = nameid;
    }

    /// <summary>
    /// Creates a new user actor.
    /// </summary>
    /// <remarks>
    /// The actor is automatically associated with the global partition, which is
    /// mandatory and cannot be removed.
    /// </remarks>
    /// <param name="nameid">The user's unique name identifier.</param>
    /// <returns>A new user <see cref="Actor"/>.</returns>
    public static Actor NewUser(Nameid nameid)
    {
        var actor = new Actor(ActorType.User, Guid.NewGuid(), nameid);
        actor.partitions.Add(new ActorPartition(actor, FargoCoreWellKnowGuids.GlobalPartitionGuid));

        actor.User = User.CreateUser(actor);

        return actor;
    }

    /// <summary>
    /// Creates the built-in administrator user actor.
    /// </summary>
    /// <remarks>
    /// The administrator is associated with the global partition, granted explicit
    /// access to the global partition and added to the built-in administrators
    /// user group. These associations are mandatory and cannot be revoked.
    /// </remarks>
    /// <param name="nameid">The administrator's unique name identifier.</param>
    /// <returns>The built-in administrator user <see cref="Actor"/>.</returns>
    public static Actor NewAdministratorUser(Nameid nameid)
    {
        var actor = new Actor(ActorType.User, FargoCoreWellKnowGuids.AdminUserGuid, nameid);
        actor.partitions.Add(new ActorPartition(actor, FargoCoreWellKnowGuids.GlobalPartitionGuid));

        actor.User = User.CreateAdministratorUser(actor);

        actor.AddPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid);

        return actor;
    }

    /// <summary>
    /// Creates a new service-account actor.
    /// </summary>
    /// <remarks>
    /// Unlike a user, a service account has no mandatory global-partition
    /// membership: its memberships are assigned explicitly.
    /// </remarks>
    /// <param name="clientId">The unique, human-readable client identifier.</param>
    /// <param name="secretHash">The hash of the client secret.</param>
    /// <returns>A new service-account <see cref="Actor"/>.</returns>
    public static Actor NewServiceAccount(Nameid clientId, PasswordHash secretHash)
    {
        var actor = new Actor(ActorType.Service, Guid.NewGuid(), clientId);

        actor.ServiceAccount = ServiceAccount.Create(actor, secretHash);

        return actor;
    }

    /// <inheritdoc/>
    public EntityType GetEntityType() => EntityType.Actor;

    /// <summary>
    /// Activates the actor.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the actor.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to deactivate the built-in administrator.
    /// </exception>
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
    /// Adds a permission to the actor if it does not already exist.
    /// </summary>
    /// <param name="action">The action type to allow.</param>
    public void AddPermission(ActionType action)
    {
        if (!permissions.Contains(action))
        {
            permissions.Add(action);
        }
    }

    /// <summary>
    /// Removes the specified permission from the actor, if it exists.
    /// </summary>
    /// <param name="action">The permission to remove.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove a permission from the built-in
    /// administrator.
    /// </exception>
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
    /// Associates the actor with the specified partition (partition membership).
    /// If the association already exists, no action is taken.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to associate an administrator with a non-global
    /// partition.
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
    /// Removes the association between the actor and the specified partition.
    /// </summary>
    /// <remarks>
    /// The global partition is mandatory for user actors and therefore cannot be
    /// removed. If the actor is not associated with the specified partition, no
    /// action is taken.
    /// </remarks>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove the global partition from a user actor.
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

    /// <summary>
    /// Grants access to the specified partition for the actor.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(new ActorPartitionAccess(this, partition));
    }

    /// <summary>
    /// Grants access to the specified partition using its identifier.
    /// </summary>
    /// <remarks>
    /// This overload is intended for internal use when the partition entity does
    /// not need to be loaded and its identifier is already known, such as a
    /// well-known partition.
    /// </remarks>
    /// <param name="partitionGuid">
    /// The identifier of the partition to which access is granted.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
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

    /// <summary>
    /// Revokes the actor's access to the specified partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the partition whose access should be revoked.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to revoke the administrator's access to the global
    /// partition.
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

    /// <summary>
    /// Validates that the actor can be deleted.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when the actor is the built-in administrator.
    /// </exception>
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
