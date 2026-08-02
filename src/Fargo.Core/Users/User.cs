using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : IEntity, IPartitioned
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
    /// Gets or sets the value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public UserAuthentication Authentication { get; private init; }

    /// <summary>
    /// Gets the permissions assigned directly to the user.
    /// </summary>
    public IReadOnlyCollection<ActionType> Permissions => permissions;

    private readonly List<ActionType> permissions = [];

    /// <summary>
    /// Gets the user groups to which the user belongs.
    /// </summary>
    public IReadOnlyCollection<UserGroup> UserGroups => userGroups;

    private readonly List<UserGroup> userGroups = [];

    /// <summary>
    /// Gets the partitions the user is allowed to access.
    /// </summary>
    public IReadOnlyCollection<Partition> PartitionAccesses => partitionAccesses;

    private readonly List<Partition> partitionAccesses = [];

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    public IReadOnlyCollection<Partition> Partitions => partitions;

    private readonly List<Partition> partitions = [];

    private User()
    {
        Authentication = new UserAuthentication(this);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="nameid">The user's unique name identifier.</param>
    /// <param name="passwordHash">The hashed password.</param>
    /// <returns>A new <see cref="User"/> instance.</returns>
    public static User CreateUser(Nameid nameid, PasswordHash passwordHash)
    {
        var user = new User
        {
            Nameid = nameid
        };

        user.Authentication.PasswordHash = passwordHash;

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

    public void AddPartition(Partition partition)
    {
        partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        partitions.Remove(partition);
    }

    public void AddUserGroup(UserGroup userGroup)
    {
        if (userGroups.Any(x => x.Guid == userGroup.Guid))
        {
            return;
        }

        userGroups.Add(userGroup);
    }

    public void RemoveUserGroup(UserGroup userGroup)
    {
        userGroups.Remove(userGroup);
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

    public void RemovePartitionAccess(Partition partition)
    {
        partitionAccesses.Remove(partition);
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
