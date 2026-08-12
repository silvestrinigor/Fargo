using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Common;
using Fargo.Core.Shared.Entities;
using Fargo.Core.Shared.Informations;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;


/// <summary>
/// Represents a user in the system.
/// </summary>
/// <remarks>
/// Every user is always associated with the global partition. The global
/// partition defines the base partition scope of the user and cannot be
/// removed.
///
/// The built-in administrator user is restricted to the global partition
/// and cannot be associated with, or removed from, any other partition.
///
/// The built-in administrator also has explicit partition access to the
/// global partition and is a member of the built-in administrators user group.
/// These associations cannot be revoked.
/// </remarks>
public class User : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
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
    public Nameid Nameid { get; set; }

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
    /// Gets the user-group memberships associated with the user.
    /// </summary>
    /// <remarks>
    /// Each entry represents the user's membership in a specific user group.
    /// </remarks>
    public IReadOnlyCollection<UserUserGroup> UserGroupMemberships => userGroupMemberships;

    private readonly List<UserUserGroup> userGroupMemberships = [];

    /// <summary>
    /// Gets the partitions to which the user has been granted direct access.
    /// </summary>
    /// <remarks>
    /// Access to descendant partitions may be inherited through the partition
    /// hierarchy and is not represented by this collection.
    ///
    /// The built-in administrator user is always granted explicit access to
    /// the global partition, and this access cannot be revoked.
    /// </remarks>
    public IReadOnlyCollection<UserPartitionAccess> PartitionAccesses => partitionAccesses;

    private readonly List<UserPartitionAccess> partitionAccesses = [];

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    /// <remarks>
    /// Every user is always associated with the global partition.
    /// The global partition defines the base partition scope of the user
    /// and cannot be removed.
    ///
    /// Additional partitions may be associated with the user.
    /// The administrator user is an exception and may only be associated
    /// with the global partition.
    /// </remarks>
    public IReadOnlyCollection<UserPartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions associated with the user.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<UserPartition> partitions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <remarks>
    /// Every user is automatically associated with the global partition.
    /// This association is mandatory and cannot be removed.
    ///
    /// Required by Entity Framework.
    /// </remarks>
    private User()
    {
        Authentication = new UserAuthentication(this);

        partitions.Add(new UserPartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
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
    /// <remarks>
    /// The administrator is automatically associated with the global partition
    /// by the constructor.
    ///
    /// The administrator is also granted explicit access to the global
    /// partition and added to the built-in administrators user group.
    ///
    /// Both associations are mandatory and cannot be revoked.
    /// </remarks>
    /// <param name="nameid">
    /// The administrator's unique name identifier.
    /// </param>
    /// <returns>The built-in administrator user.</returns>
    public static User CreateAdministratorUser(Nameid nameid)
    {
        var user = new User
        {
            Guid = FargoCoreWellKnowGuids.AdminUserGuid,
            Nameid = nameid
        };

        user.AddPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid);

        user.AddUserGroup(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid);

        return user;
    }

    /// <summary>
    /// Associates the user with the specified partition.
    /// If the partition is already associated, no action is taken.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when attempting to associate an administrator with a
    /// non-global partition.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (IsAdmin && !partition.IsGlobalPartition)
        {
            throw new FargoCoreException(
                $"Cannot associate the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}' with the non-global partition '{partition.Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new UserPartition(this, partition));
    }

    /// <summary>
    /// Removes the association between the user and the specified partition.
    /// </summary>
    /// <remarks>
    /// The global partition is mandatory for every user and therefore cannot
    /// be removed.
    ///
    /// If the user is not associated with the specified partition,
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
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' is mandatory and cannot be removed from the user '{Guid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <summary>
    /// Adds the user to the specified user group.
    /// If the user already belongs to the group, no action is taken.
    /// </summary>
    /// <param name="userGroup">The user group to add.</param>
    public void AddUserGroup(UserGroup userGroup)
    {
        ArgumentNullException.ThrowIfNull(userGroup);

        if (userGroupMemberships.Any(x => x.UserGroupGuid == userGroup.Guid))
        {
            return;
        }

        userGroupMemberships.Add(new UserUserGroup(this, userGroup));
    }

    /// <summary>
    /// Adds the user to the specified user group using its identifier.
    /// </summary>
    /// <remarks>
    /// This overload is intended for internal use when the user group entity
    /// does not need to be loaded and only its identifier is known, such as
    /// when assigning membership to a well-known user group.
    /// </remarks>
    /// <param name="userGroupGuid">
    /// The identifier of the user group to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userGroupGuid"/> is
    /// <see cref="Guid.Empty"/>.
    /// </exception>
    internal void AddUserGroup(Guid userGroupGuid)
    {
        if (userGroupGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "User group GUID cannot be empty.",
                nameof(userGroupGuid));
        }

        if (userGroupMemberships.Any(x => x.UserGroupGuid == userGroupGuid))
        {
            return;
        }

        userGroupMemberships.Add(new UserUserGroup(this, userGroupGuid));
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
        if (IsAdmin && userGroupGuid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid)
        {
            throw new FargoCoreException(
                $"Cannot remove the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}' from the administrators user group '{FargoCoreWellKnowGuids.AdministratorsUserGroupGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        userGroupMemberships.RemoveAll(g => g.UserGroupGuid == userGroupGuid);
    }

    /// <summary>
    /// Grants access to the specified partition for the user.
    /// </summary>
    /// <param name="partition">The partition to grant access to.</param>
    public void AddPartitionAccess(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitionAccesses.Add(new UserPartitionAccess(this, partition));
    }

    /// <summary>
    /// Grants access to the specified partition using its identifier.
    /// </summary>
    /// <remarks>
    /// This overload is intended for internal use when the partition entity
    /// does not need to be loaded and its identifier is already known, such as
    /// when assigning access to a well-known partition.
    /// </remarks>
    /// <param name="partitionGuid">
    /// The identifier of the partition to which access is granted.
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

        if (partitionAccesses.Any(x => x.PartitionGuid == partitionGuid))
        {
            return;
        }

        partitionAccesses.Add(new UserPartitionAccess(this, partitionGuid));
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
                $"Cannot revoke the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}' access to the global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        partitionAccesses.RemoveAll(p => p.PartitionGuid == partitionGuid);
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
                $"Cannot revoke permission '{action}' from the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}'.",
                FargoErrorType.InvalidOperation);
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
                $"Cannot deactivate the admin user '{FargoCoreWellKnowGuids.AdminUserGuid}'.",
                FargoErrorType.InvalidOperation);
        }

        IsActive = false;
    }

    public EntityType GetEntityType() => EntityType.User;
}
