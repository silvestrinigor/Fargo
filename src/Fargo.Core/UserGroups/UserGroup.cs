using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Informations;

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
public class UserGroup : IEntity, IPartitionedReadOnly
{
    /// <summary>
    /// Gets the unique identifier of the user group.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the value indicating whether the user group is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether this is the built-in administrators user group.
    /// </summary>
    public bool IsAdminUserGroup => Guid == FargoCoreGuids.AdminUserGroupGuid;

    /// <summary>
    /// Gets or sets the unique nameid of the user group.
    /// </summary>
    public Nameid Nameid { get; set; }

    /// <summary>
    /// Gets or sets the textual description associated with the user group.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the permissions granted to the user group.
    /// </summary>
    public IReadOnlyCollection<ActionType> Permissions => permissions;
    private readonly List<ActionType> permissions = [];

    /// <summary>
    /// Gets the partitions associated with the user group.
    /// </summary>
    public IReadOnlyCollection<Partition> Partitions => partitions;
    private readonly List<Partition> partitions = [];

    /// <summary>
    /// Gets the partition access entries associated with the user group.
    /// </summary>
    /// <remarks>
    /// These entries determine which partitions members of the group may access.
    /// </remarks>
    public IReadOnlyCollection<Partition> PartitionAccesses => partitionAccesses;
    private readonly List<Partition> partitionAccesses = [];

    private UserGroup()
    {
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
    /// <param name="nameid">The unique name identifier of the user group.</param>
    /// <returns>The administrators <see cref="UserGroup"/>.</returns>
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
        if (partitionAccesses.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(partition);
    }

    /// <summary>
    /// Removes partition access from the user group if it exists.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        partitionAccesses.RemoveAll(p => p.Guid == partitionGuid);
    }

    /// <summary>
    /// Associates the user group with the specified partition if it is not already associated.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    public void AddPartition(Partition partition)
    {
        if (partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        partitions.Add(partition);
    }

    /// <summary>
    /// Removes the association between the user group and the specified partition.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    public void RemovePartition(Guid partitionGuid)
    {
        partitions.RemoveAll(p => p.Guid == partitionGuid);
    }

    /// <summary>
    /// Adds a permission to the user group if it does not already exist.
    /// </summary>
    /// <param name="action">The action to grant to the user group.</param>
    public void AddPermission(ActionType action)
    {
        if (permissions.Any(p => p == action))
        {
            return;
        }

        permissions.Add(action);
    }

    /// <summary>
    /// Removes a permission from the user group if it exists.
    /// </summary>
    /// <param name="action">The action to remove from the user group.</param>
    public void RemovePermission(ActionType action)
    {
        permissions.RemoveAll(a => a == action);
    }
}
