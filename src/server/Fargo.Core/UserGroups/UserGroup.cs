using Fargo.Core.Actors;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;

namespace Fargo.Core.UserGroups;

/// <summary>
/// Represents a user group in the system.
/// </summary>
/// <remarks>
/// A user group defines a set of permissions that determine which actions
/// its members are allowed to perform.
///
/// A user may access the group only if they have access to at least one of the
/// partitions associated with it, subject to additional authorization rules.
///
/// Every user group is always associated with the global partition. The global
/// partition defines the base partition scope of the user group and cannot be
/// removed.
///
/// The built-in administrators user group is restricted to the global partition
/// and cannot be associated with, or removed from, any other partition.
///
/// The built-in administrators user group also has explicit partition access to
/// the global partition. This access cannot be revoked.
/// </remarks>
public class UserGroup : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
{
    /// <summary>
    /// Gets the unique identifier of the user group.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the value indicating whether the user group is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether this is the built-in administrators user group.
    /// </summary>
    public bool IsAdministrators => Guid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid;

    /// <summary>
    /// Gets or sets the unique nameid of the user group.
    /// </summary>
    public Nameid Nameid { get; set; }

    /// <summary>
    /// Gets or sets the textual description associated with the user group.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the unique identifier of the parent user group, if one is assigned.
    /// </summary>
    public Guid? ParentUserGroupGuid { get; private set; }

    /// <summary>
    /// Gets the parent user group, if one is assigned.
    /// </summary>
    public UserGroup? ParentUserGroup { get; private set; }

    /// <summary>
    /// Gets the permissions granted to the user group.
    /// </summary>
    public IReadOnlyCollection<ActionType> Permissions => permissions;

    private readonly List<ActionType> permissions = [];

    /// <summary>
    /// Gets the partitions associated with the user group.
    /// </summary>
    /// <remarks>
    /// Every user group is always associated with the global partition.
    /// The global partition defines the base partition scope of the group
    /// and cannot be removed.
    ///
    /// Additional partitions may be associated with the user group.
    /// The administrators user group is an exception and may only be
    /// associated with the global partition.
    /// </remarks>
    public IReadOnlyCollection<UserGroupPartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions associated with the
    /// user group.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<UserGroupPartition> partitions = [];

    /// <summary>
    /// Gets the partition access entries associated with the user group.
    /// </summary>
    /// <remarks>
    /// These entries determine which partitions members of the group are granted
    /// direct access to. Access to descendant partitions may be inherited through
    /// the partition hierarchy.
    /// </remarks>
    public IReadOnlyCollection<UserGroupPartitionAccess> PartitionAccesses => partitionAccesses;

    private readonly List<UserGroupPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Initializes a new user group.
    /// </summary>
    /// <remarks>
    /// Every user group is automatically associated with the global partition.
    /// This association is mandatory and cannot be removed.
    /// </remarks>
    private UserGroup()
    {
        partitions.Add(new UserGroupPartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    /// <summary>
    /// Creates a new user group with the specified <paramref name="nameid"/>.
    /// </summary>
    /// <param name="nameid">The unique name identifier of the user group.</param>
    /// <returns>A new <see cref="UserGroup"/> instance.</returns>
    public static UserGroup CreateUserGroup(Nameid nameid)
    {
        var usergroup = new UserGroup
        {
            Nameid = nameid
        };

        return usergroup;
    }

    /// <summary>
    /// Creates the built-in administrators user group.
    /// </summary>
    /// <remarks>
    /// The administrators user group is automatically associated with the global
    /// partition and is additionally granted explicit access to the global
    /// partition.
    ///
    /// The global partition access is mandatory and cannot be revoked.
    /// </remarks>
    /// <param name="nameid">
    /// The unique name identifier of the user group.
    /// </param>
    /// <returns>The administrators <see cref="UserGroup"/>.</returns>
    public static UserGroup CreateAdministratorsUserGroup(Nameid nameid)
    {
        var administratorsUsergroup = new UserGroup
        {
            Guid = FargoCoreWellKnowGuids.AdministratorsUserGroupGuid,
            Nameid = nameid
        };

        administratorsUsergroup.AddPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid);

        return administratorsUsergroup;
    }

    /// <summary>
    /// Adds partition access to the user group if it does not already exist.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(new UserGroupPartitionAccess(this, partition));
    }

    /// <summary>
    /// Adds partition access to the user group if it does not already exist.
    /// </summary>
    /// <remarks>
    /// This overload is intended for internal use when the partition entity does
    /// not need to be loaded and only its identifier is available.
    /// </remarks>
    /// <param name="partitionGuid">
    /// The identifier of the partition to grant access to.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="partitionGuid"/> is
    /// <see cref="Guid.Empty"/>.
    /// </exception>
    internal void AddPartitionAccess(Guid partitionGuid)
    {
        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        if (partitionAccesses.Any(p => p.PartitionGuid == partitionGuid))
        {
            return;
        }

        partitionAccesses.Add(new UserGroupPartitionAccess(this, partitionGuid));
    }

    /// <summary>
    /// Revokes the user group's access to the specified partition.
    /// If the user group does not have access to the partition, no action is taken.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the partition whose access should be revoked.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to revoke the administrators user group's access to
    /// the global partition.
    /// </exception>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        if (IsAdministrators && partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"Cannot revoke the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}' access to the global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitionAccesses.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <summary>
    /// Associates the user group with the specified partition if it is not
    /// already associated.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to associate the administrators user group with
    /// a non-global partition.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (IsAdministrators && !partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"Cannot associate the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}' with the non-global partition '{partition.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new UserGroupPartition(this, partition));
    }

    /// <summary>
    /// Removes the association between the user group and the specified
    /// partition.
    /// </summary>
    /// <remarks>
    /// The global partition is mandatory for every user group and therefore
    /// cannot be removed.
    ///
    /// The administrators user group is additionally restricted to the global
    /// partition and therefore cannot be associated with any other partition.
    ///
    /// If the user group is not associated with the specified partition,
    /// no action is taken.
    /// </remarks>
    /// <param name="partitionGuid">
    /// The identifier of the partition to remove.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove the global partition.
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' is mandatory and cannot be removed from the user group '{Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <summary>
    /// Adds a permission to the user group if it does not already exist.
    /// </summary>
    /// <param name="action">The action to grant to the user group.</param>
    public void AddPermission(ActionType action)
    {
        if (permissions.Contains(action))
        {
            return;
        }

        permissions.Add(action);
    }

    /// <summary>
    /// Removes the specified permission from the user group.
    /// </summary>
    /// <param name="action">
    /// The permission to remove from the user group.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove a permission from the administrators user group.
    /// </exception>
    public void RemovePermission(ActionType action)
    {
        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"Cannot revoke the permission '{action}' from the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        permissions.RemoveAll(a => a == action);
    }

    /// <summary>
    /// Activates the user group.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the user group.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to deactivate the administrators user group.
    /// </exception>
    public void Deactivate()
    {
        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"Cannot deactivate the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        IsActive = false;
    }

    /// <summary>
    /// Assigns the specified user group as the parent of this user group.
    /// </summary>
    /// <param name="parentUserGroup">
    /// The user group to assign as the parent.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parentUserGroup"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to assign a parent to the built-in administrators
    /// user group, or when attempting to assign the user group as its own parent.
    /// </exception>
    /// <remarks>
    /// This method validates only invariants that can be enforced by this
    /// <see cref="UserGroup"/> instance. It does <b>not</b> validate the overall
    /// user group hierarchy or detect cyclic parent-child relationships.
    ///
    /// Before calling this method, the application should validate the proposed
    /// assignment using <see cref="UserGroupService"/> to ensure that assigning
    /// <paramref name="parentUserGroup"/> as the parent will not introduce an
    /// invalid hierarchy.
    /// </remarks>
    public void SetParentUserGroup(UserGroup parentUserGroup)
    {
        ArgumentNullException.ThrowIfNull(parentUserGroup);

        if (IsAdministrators)
        {
            throw new FargoCoreException(
                $"Administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}' cannot be added inside another user group '{parentUserGroup.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        if (parentUserGroup.Guid == Guid)
        {
            throw new FargoCoreException(
                $"The user group '{Guid}' cannot be its own parent.",
                FargoErrorType.InvalidOperation);
        }

        ParentUserGroup = parentUserGroup;
        ParentUserGroupGuid = parentUserGroup.Guid;
    }

    /// <summary>
    /// Removes the parent user group association from this user group.
    /// </summary>
    /// <remarks>
    /// If the user group does not currently have a parent, this method has no
    /// effect.
    ///
    /// A user group without a parent is considered a root user group within the
    /// user group hierarchy.
    /// </remarks>
    public void RemoveFromParentUserGroup()
    {
        ParentUserGroup = null;
        ParentUserGroupGuid = null;
    }

    public EntityType GetEntityType() => EntityType.UserGroup;
}
