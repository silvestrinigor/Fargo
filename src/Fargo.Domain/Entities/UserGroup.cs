using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities;

/// <summary>
/// Represents a user group that can be assigned permissions and associated with users.
/// </summary>
public class UserGroup
{
    /// <summary>
    /// Gets the unique identifier (GUID) for the user.
    /// </summary>
    public Guid Guid
    {
        get;
        init;
    } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique identifier (NAMEID) of the user.
    /// </summary>
    public required Nameid Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the description of the user group.
    /// </summary>
    public Description Description
    {
        get;
        set;
    } = Description.Empty;

    public IReadOnlyCollection<UserGroupPermission> Permissions => permissions;

    private readonly List<UserGroupPermission> permissions = [];

    public void AddPermission(ActionType action)
    {
        if (permissions.Any(p => p.ActionType == action))
            return;

        var permissionToAdd = new UserGroupPermission
        {
            UserGroup = this,
            ActionType = action
        };

        permissions.Add(permissionToAdd);
    }

    public void RemovePermission(ActionType action)
    {
        var permissionToRemove = permissions.SingleOrDefault(p => p.ActionType == action);

        if (permissionToRemove is null)
            return;

        permissions.Remove(permissionToRemove);
    }

    public IReadOnlyCollection<User> Users => users;

    private readonly List<User> users = [];

    public void AddUser(User user)
    {
        if (users.Contains(user))
            return;

        users.Add(user);
    }

    public void RemoveUser(User user)
    {
        if (!users.Contains(user))
            return;

        users.Remove(user);
    }

    public PartitionCollection Partitions
    {
        get;
        init;
    } = [];
}