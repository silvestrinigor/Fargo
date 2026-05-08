using Fargo.Domain.Partitions;

namespace Fargo.Domain.Users;

// TODO: organize the class
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
