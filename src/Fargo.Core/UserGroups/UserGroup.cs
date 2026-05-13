using Fargo.Core.Partitions;
using Fargo.Core.Users;
using System.Collections.ObjectModel;

namespace Fargo.Core.UserGroups;

#region Entity

/// <summary>
/// Represents a user group in the system.
/// </summary>
/// <remarks>
/// A user group defines a set of permissions that determine which actions
/// its members are allowed to perform.
///
/// A user group is partitioned data and may belong to multiple
/// <see cref="Partition"/> instances.
///
/// A user may access the group only if they have access to at least one of the
/// partitions associated with it, subject to additional authorization rules.
/// </remarks>
public class UserGroup : ModifiedEntity, IPartitionedEntity, IPartitionUser, IPermissionUser, IActivable
{
    /// <summary>
    /// Gets or sets the unique NAMEID of the user group.
    /// </summary>
    /// <remarks>
    /// This value identifies the group in the system and must satisfy
    /// the validation rules defined by <see cref="Nameid"/>.
    /// </remarks>
    public required Nameid Nameid { get; set; }

    /// <summary>
    /// Gets or sets the textual description associated with the user group.
    /// </summary>
    /// <remarks>
    /// Provides additional contextual information about the group.
    /// Defaults to <see cref="Description.Empty"/> when not specified.
    /// </remarks>
    public Description Description { get; set; } = Description.Empty;

    #region Active

    /// <summary>
    /// Gets a value indicating whether the user group is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the user group.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the user group.
    /// </summary>
    public void Deactivate() => IsActive = false;

    #endregion Active

    #region Permission

    /// <summary>
    /// Gets the read-only collection of permissions assigned to the user group.
    /// </summary>
    /// <remarks>
    /// Represents the permissions granted by the group and is used to determine
    /// which actions members of the group are allowed to perform.
    /// </remarks>
    public IReadOnlyCollection<UserGroupPermission> Permissions
    {
        get => userGroupPermissions;
        init => userGroupPermissions = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPermission> IPermissionUser.Permissions => Permissions;

    private readonly List<UserGroupPermission> userGroupPermissions = [];

    /// <summary>
    /// Adds a permission to the user group if it does not already exist.
    /// </summary>
    /// <param name="action">The action to grant to the user group.</param>
    public void AddPermission(ActionType action)
    {
        if (userGroupPermissions.Any(x => x.Action == action))
        {
            return;
        }

        var userGroupPermission = new UserGroupPermission
        {
            Action = action,
            UserGroup = this
        };

        userGroupPermissions.Add(userGroupPermission);
    }

    /// <summary>
    /// Removes a permission from the user group if it exists.
    /// </summary>
    /// <param name="action">The action to remove from the user group.</param>
    public void RemovePermission(ActionType action)
    {
        var userGroupPermission = userGroupPermissions
            .SingleOrDefault(x => x.Action == action);

        if (userGroupPermission == null)
        {
            return;
        }

        userGroupPermissions.Remove(userGroupPermission);
    }

    #endregion Permission

    #region Partition

    /// <summary>
    /// Gets the partitions associated with the user group.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the group and are used
    /// for partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;

    /// <summary>
    /// Gets the partition access entries associated with the user group.
    /// </summary>
    /// <remarks>
    /// These entries define which partitions the group has access to and are
    /// used in access evaluation logic.
    /// </remarks>
    public IReadOnlyCollection<UserGroupPartitionAccess> PartitionAccesses
    {
        get => partitionAccesses;
        init => partitionAccesses = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionAccess> IPartitionUser.PartitionAccesses => PartitionAccesses;

    private readonly List<UserGroupPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Adds partition access to the user group if it does not already exist.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        var partitionAccess = new UserGroupPartitionAccess
        {
            UserGroup = this,
            Partition = partition
        };

        partitionAccesses.Add(partitionAccess);
    }

    /// <summary>
    /// Removes partition access from the user group if it exists.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        var userGroupPartition =
            partitionAccesses.SingleOrDefault(x => x.PartitionGuid == partitionGuid);

        if (userGroupPartition == null)
        {
            return;
        }

        partitionAccesses.Remove(userGroupPartition);
    }

    #endregion Partition

    #region User

    /// <summary>
    /// Gets the read-only collection of users associated with the user group.
    /// </summary>
    /// <remarks>
    /// Represents users that belong to the group.
    /// This collection is intended for navigation and persistence purposes.
    /// Membership changes should be controlled through explicit domain behaviors.
    /// </remarks>
    public IReadOnlyCollection<User> Users
    {
        get => users;
        init => users = [.. value];
    }

    private readonly List<User> users = [];

    #endregion User
}

#endregion Entity

#region Collections

/// <summary>
/// Represents a collection of <see cref="UserGroup"/> instances.
/// </summary>
/// <remarks>
/// This collection enforces domain rules for user groups, such as preventing
/// duplicate items and rejecting <see langword="null"/> values.
/// </remarks>
public sealed class UserGroupCollection : Collection<UserGroup>
{
    /// <summary>
    /// Initializes an empty <see cref="UserGroupCollection"/>.
    /// </summary>
    public UserGroupCollection()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="UserGroupCollection"/> with the specified groups.
    /// </summary>
    /// <param name="groups">
    /// The groups used to populate the collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="groups"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when duplicate groups are found.
    /// </exception>
    public UserGroupCollection(IEnumerable<UserGroup> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        foreach (var group in groups)
        {
            Add(group);
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group already exists in the collection.
    /// </exception>
    protected override void InsertItem(int index, UserGroup item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item))
        {
            throw new InvalidOperationException(
                "The user group already exists in the collection.");
        }

        base.InsertItem(index, item);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group already exists in the collection.
    /// </exception>
    protected override void SetItem(int index, UserGroup item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Items.Contains(item) && !ReferenceEquals(Items[index], item))
        {
            throw new InvalidOperationException(
                "The user group already exists in the collection.");
        }

        base.SetItem(index, item);
    }
}

#endregion Collections

#region Permissions

/// <summary>
/// Represents a permission assigned to a user group.
/// </summary>
/// <remarks>
/// Each instance defines that a specific <see cref="UserGroup"/> is allowed
/// to perform a particular <see cref="ActionType"/>.
///
/// This entity is part of the <see cref="UserGroup"/> aggregate and represents
/// a single permission entry associated with the group.
///
/// The entity also implements <see cref="IModifiedEntityMember"/>, meaning
/// that any changes to this permission will propagate auditing updates
/// to the parent <see cref="UserGroup"/> entity.
/// </remarks>
public class UserGroupPermission : Entity, IModifiedEntityMember, IPermission
{
    /// <summary>
    /// Gets the unique identifier of the user group that owns this permission.
    /// </summary>
    /// <remarks>
    /// This value mirrors the identifier of the associated <see cref="UserGroup"/>.
    /// It is automatically synchronized when the <see cref="UserGroup"/> property
    /// is assigned.
    /// </remarks>
    public Guid UserGroupGuid { get; private set; }

    /// <summary>
    /// Gets the user group associated with this permission.
    /// </summary>
    /// <remarks>
    /// When the group is assigned, the <see cref="UserGroupGuid"/> property
    /// is automatically synchronized with the group's identifier.
    ///
    /// This navigation property represents the parent entity in the
    /// aggregate relationship.
    /// </remarks>
    public required UserGroup UserGroup
    {
        get;
        init
        {
            UserGroupGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the action that the user group is allowed to perform.
    /// </summary>
    /// <remarks>
    /// Each permission grants the associated user group the ability to perform
    /// the specified <see cref="ActionType"/>.
    /// </remarks>
    public required ActionType Action { get; init; }

    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this permission changes.
    /// </summary>
    /// <remarks>
    /// Since permissions are part of the <see cref="UserGroup"/> aggregate,
    /// modifications to this entity should update the audit metadata of
    /// the parent <see cref="UserGroup"/>.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => UserGroup;
}

#endregion Permissions

#region Partition Access

/// <summary>
/// Represents the access relationship between a <see cref="UserGroup"/> and a <see cref="Partition"/>.
/// </summary>
/// <remarks>
/// A <see cref="UserGroupPartitionAccess"/> defines whether a user group is allowed
/// to access a specific partition.
///
/// Partitions are used to logically isolate data in the system. Users that belong
/// to a group inherit access to the partitions associated with that group through
/// <see cref="UserGroupPartitionAccess"/>.
///
/// This entity is a member of the <see cref="UserGroup"/> aggregate and implements
/// <see cref="IModifiedEntityMember"/>, meaning that any modification to this
/// entity should update the audit metadata of the parent <see cref="UserGroup"/>.
/// </remarks>
public class UserGroupPartitionAccess : Entity, IModifiedEntityMember, IPartitionAccess
{
    /// <summary>
    /// Gets the unique identifier of the user group associated with this access entry.
    /// </summary>
    public Guid UserGroupGuid { get; private set; }

    /// <summary>
    /// Gets or sets the user group that owns this partition access.
    /// </summary>
    /// <remarks>
    /// When the user group reference is assigned, <see cref="UserGroupGuid"/> is
    /// automatically synchronized with the group identifier.
    /// </remarks>
    public required UserGroup UserGroup
    {
        get;
        set
        {
            UserGroupGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the unique identifier of the partition associated with this access entry.
    /// </summary>
    public Guid PartitionGuid { get; private set; }

    /// <summary>
    /// Gets or sets the partition to which the user group has access.
    /// </summary>
    /// <remarks>
    /// When the partition reference is assigned, <see cref="PartitionGuid"/>
    /// is automatically synchronized with the partition identifier.
    /// </remarks>
    public required Partition Partition
    {
        get;
        set
        {
            PartitionGuid = value.Guid;
            field = value;
        }
    }

    /// <summary>
    /// Gets the parent audited entity whose audit metadata must be updated
    /// when this entity changes.
    /// </summary>
    /// <remarks>
    /// Since <see cref="UserGroupPartitionAccess"/> belongs to the
    /// <see cref="UserGroup"/> aggregate, the parent audited entity is the
    /// associated user group.
    /// </remarks>
    public IModifiedEntity ParentEditedEntity => UserGroup;
}

#endregion Partition Access
