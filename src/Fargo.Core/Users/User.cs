using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Actions;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : IEntity, IPartitionedReadOnly
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets a value indicating whether the user is the main admin.
    /// </summary>
    public bool IsAdmin => Guid == FargoCoreGuids.AdminUserGuid;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the unique nameid of the user.
    /// </summary>
    public required Nameid Nameid { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public FirstName? FirstName { get; set; } = null;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public LastName? LastName { get; set; } = null;

    /// <summary>
    /// Gets or sets the textual description associated with the user.
    /// </summary>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the authentication information associated with the user.
    /// </summary>
    public UserAuthentication Authentication { get; private init; }

    /// <summary>
    /// Gets the permissions assigned directly to the user.
    /// Permissions inherited from user groups are not included.
    /// </summary>
    public IReadOnlyCollection<ActionType> Permissions => permissions;
    private readonly List<ActionType> permissions = [];

    /// <summary>
    /// Gets the user groups to which the user belongs.
    /// </summary>
    public IReadOnlyCollection<UserGroup> UserGroups => userGroups;
    private readonly List<UserGroup> userGroups = [];

    /// <summary>
    /// Gets the partitions to which the user has been granted direct access.
    /// </summary>
    public IReadOnlyCollection<Partition> PartitionAccesses => partitionAccesses;
    private readonly List<Partition> partitionAccesses = [];

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    public IReadOnlyCollection<Partition> Partitions => partitions;
    private readonly List<Partition> partitions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// Intended only for entity creation through factory methods and Entity Framework.
    /// </summary>
    private User()
    {
        Authentication = new UserAuthentication(this);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="nameid">The user's unique name identifier.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    public static User CreateUser(Nameid nameid)
    {
        var user = new User
        {
            Nameid = nameid
        };

        return user;
    }

    /// <summary>
    /// Creates the built-in administrator user.
    /// </summary>
    /// <param name="nameid">The administrator's unique name identifier.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <returns>The built-in administrator user.</returns>
    public static User CreateAdministratorUser(Nameid nameid, PasswordHash passwordHash)
    {
        var user = new User
        {
            Guid = FargoCoreGuids.AdminUserGuid,
            Nameid = nameid
        };

        user.Authentication.PasswordHash = passwordHash;

        return user;
    }

    /// <summary>
    /// Associates the user with the specified partition.
    /// If the partition is already associated, no action is taken.
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
    /// Removes the association between the user and the specified partition.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition to remove.</param>
    public void RemovePartition(Guid partitionGuid)
    {
        partitions.RemoveAll(p => p.Guid == partitionGuid);
    }

    /// <summary>
    /// Adds the user to the specified user group.
    /// If the user already belongs to the group, no action is taken.
    /// </summary>
    /// <param name="userGroup">The user group to add.</param>
    public void AddUserGroup(UserGroup userGroup)
    {
        if (userGroups.Any(x => x.Guid == userGroup.Guid))
        {
            return;
        }

        userGroups.Add(userGroup);
    }

    /// <summary>
    /// Removes the user from the specified user group.
    /// </summary>
    /// <param name="userGroupGuid">The identifier of the user group to remove.</param>
    public void RemoveUserGroup(Guid userGroupGuid)
    {
        userGroups.RemoveAll(g => g.Guid == userGroupGuid);
    }

    /// <summary>
    /// Grants access to the specified partition for the user.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.Guid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(partition);
    }

    /// <summary>
    /// Revokes the user's access to the specified partition.
    /// </summary>
    /// <param name="partitionGuid">The identifier of the partition.</param>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        partitionAccesses.RemoveAll(p => p.Guid == partitionGuid);
    }

    /// <summary>
    /// Adds a permission to the user if it does not already exist.
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
    /// Removes a permission from the user if it exists.
    /// </summary>
    /// <param name="action">The action type to remove.</param>
    public void RemovePermission(ActionType action)
    {
        permissions.RemoveAll(x => x == action);
    }
}
