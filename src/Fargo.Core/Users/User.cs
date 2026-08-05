using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Informations;
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
    public bool IsAdmin => Guid == FargoCoreWellKnowGuids.AdminUserGuid;

    /// <summary>
    /// Gets a value indicating whether the user account is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

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
            Guid = FargoCoreWellKnowGuids.AdminUserGuid,
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
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to associate an administrator with a non-global partition.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        if (IsAdmin && !partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                "Cannot associate the admin user with a non-global partition.",
                FargoCoreErrorType.InvalidOperation);
        }

        if (partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        partitions.Add(partition);
    }

    /// <summary>
    /// Removes the association between the user and the specified partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// The identifier of the partition to remove.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove an administrator from the global partition.
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (IsAdmin && partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                "Cannot remove the admin user from the global partition.",
                FargoCoreErrorType.InvalidOperation);
        }

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
    /// <param name="userGroupGuid">
    /// The identifier of the user group to remove.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove an administrator from the administrators
    /// user group.
    /// </exception>
    public void RemoveUserGroup(Guid userGroupGuid)
    {
        if (IsAdmin && userGroupGuid == FargoCoreWellKnowGuids.AdminUserGroupGuid)
        {
            throw new FargoCoreException(
                "Cannot remove the admin user from the administrators user group.",
                FargoCoreErrorType.InvalidOperation);
        }

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
    /// <param name="partitionGuid">
    /// The identifier of the partition whose access should be revoked.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to revoke the administrator's access to the global
    /// partition.
    /// </exception>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        if (IsAdmin && partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                "Cannot remove the global partition access from the admin user.",
                FargoCoreErrorType.InvalidOperation);
        }

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
    /// Removes the specified permission from the user, if it exists.
    /// </summary>
    /// <param name="action">
    /// The permission to remove.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to remove a permission from an administrator.
    /// </exception>
    public void RemovePermission(ActionType action)
    {
        if (IsAdmin)
        {
            throw new FargoCoreException(
                "Cannot remove any permission from the admin user.",
                FargoCoreErrorType.InvalidOperation);
        }

        permissions.RemoveAll(x => x == action);
    }

    /// <summary>
    /// Activates the user.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the user.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to deactivate an administrator.
    /// </exception>
    public void Deactivate()
    {
        if (IsAdmin)
        {
            throw new FargoCoreException(
                "Cannot deactivate the admin user.",
                FargoCoreErrorType.InvalidOperation);
        }

        IsActive = false;
    }
}
