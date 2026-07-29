using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;

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
/// </remarks>
public class UserGroup : IEntity, IPartitioned
{
    /// <summary>
    /// Gets the unique identifier of the user group.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique nameid of the user group.
    /// </summary>
    public Nameid Nameid { get; set; }

    /// <summary>
    /// Gets or sets the textual description associated with the user group.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets or sets the value indicating whether the user group is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    private readonly List<UserGroupPermission> userGroupPermissions = [];

    /// <summary>
    /// Gets the read-only collection of permissions assigned to the user group.
    /// </summary>
    public IReadOnlyCollection<UserGroupPermission> Permissions => userGroupPermissions;

    private readonly List<Partition> partitions = [];

    /// <summary>
    /// Gets the partition associated with the user group.
    /// </summary>
    public IReadOnlyCollection<Partition> Partitions => partitions;

    private readonly List<Partition> partitionAccesses = [];

    /// <summary>
    /// Gets the partition access entries associated with the user group.
    /// </summary>
    /// <remarks>
    /// These entries define which partitions the group has access to and are
    /// used in access evaluation logic.
    /// </remarks>
    public IReadOnlyCollection<Partition> PartitionAccesses => partitionAccesses;

    private UserGroup()
    {
    }

    /// <summary>
    /// Creates a new user group.
    /// </summary>
    public static UserGroup CreateUserGroup(Nameid nameid)
    {
        var usergroup = new UserGroup
        {
            Nameid = nameid
        };

        return usergroup;
    }

    /// <summary>
    /// Creates the administrators user group.
    /// </summary>
    public static UserGroup CreateAdministratorsUserGroup(Nameid nameid)
    {
        var administratorsUsergroup = new UserGroup
        {
            Guid = FargoCoreGuids.AdminUserGroupGuid,
            Nameid = nameid
        };

        return administratorsUsergroup;
    }

    /// <summary>
    /// Adds partition access to the user group if it does not already exist.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    public void AddPartitionAccess(Partition partition)
    {
        partitionAccesses.Add(partition);
    }


    /// <summary>
    /// Removes partition access from the user group if it exists.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    public void RemovePartitionAccess(Partition partition)
    {
        partitionAccesses.Remove(partition);
    }

    public void AddPartition(Partition partition)
    {
        partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        partitions.Remove(partition);
    }

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
}
