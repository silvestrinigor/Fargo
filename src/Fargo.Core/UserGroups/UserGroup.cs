using Fargo.Core.Activables;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Users;

namespace Fargo.Core.UserGroups;

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
public class UserGroup : Entity, IEntityTyped, IPartitioned, IPartitionUser, IPermissionUser, IActivable
{
    private UserGroup()
    {
    }

    public UserGroup(Nameid nameid, Description? description = null)
    {
        Nameid = nameid;
        Description = description ?? Description.Empty;
    }

    public static UserGroup CreateUserGroup(Nameid nameid, Description? description = null)
        => new(nameid, description);

    /// <summary>
    /// Gets or sets the unique NAMEID of the user group.
    /// </summary>
    public Nameid Nameid { get; set; }

    /// <summary>
    /// Gets or sets the textual description associated with the user group.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets a value indicating whether the user group is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public EntityType GetEntityType() => EntityType.UserGroup;

    /// <summary>
    /// Gets the read-only collection of permissions assigned to the user group.
    /// </summary>
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

    /// <summary>
    /// Gets the partitions associated with the user group.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the group and are used
    /// for partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;

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

    public void AddPartition(Partition partition)
    {
        Partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        Partitions.Remove(partition);
    }

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
}
