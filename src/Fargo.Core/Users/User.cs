using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity, IPartitioned
{
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

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    public required PasswordHash PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the default password expiration perid.
    /// </summary>
    public TimeSpan? DefaultPasswordExpirationPeriod { get; set; } = null;

    /// <summary>
    /// Gets or sets the required date to change the password.
    /// </summary>
    public DateTimeOffset? RequirePasswordChangeAt { get; set; } = null;

    /// <summary>
    /// Gets a value indicating whether it is necessary to change password.
    /// </summary>
    public bool IsPasswordChangeRequired
        => RequirePasswordChangeAt is not null && DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

    /// <summary>
    /// 
    /// </summary>
    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

    private readonly List<UserPermission> permissions = [];

    /// <summary>
    /// Gets the read-only collection of permissions assigned directly to the user.
    ///
    /// Each permission represents an allowed <see cref="ActionType"/>
    /// that the user can perform without considering group memberships.
    /// </summary>
    public IReadOnlyCollection<UserPermission> Permissions => permissions;

    private readonly List<UserGroup> userGroups = [];

    public IReadOnlyCollection<UserGroup> UserGroups => userGroups;

    /// <summary>
    /// Gets the read-only collection of partitions the user has access to.
    /// </summary>
    /// <remarks>
    /// Partitions define logical boundaries in the system.
    /// A user can access entities that have no partition (public), or that
    /// belong to at least one partition to which the user has been granted access.
    /// </remarks>
    public IReadOnlyCollection<UserPartitionAccess> PartitionAccesses
    {
        get => partitionAccesses;
        init => partitionAccesses = [.. value];
    }

    private readonly List<UserPartitionAccess> partitionAccesses = [];

    private readonly List<Partition> partitions = [];

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    public IReadOnlyCollection<Partition> Partitions => partitions;

    private User()
    {
    }

    public static User CreateUser(Nameid nameid, PasswordHash passwordHash)
    {
        var user = new User
        {
            Nameid = nameid,
            PasswordHash = passwordHash
        };

        return user;
    }

    public static User CreateAdministratorUser(Nameid nameid, PasswordHash passwordHash)
    {
        var user = new User
        {
            Guid = FargoCoreGuids.AdminUserGuid,
            Nameid = nameid,
            PasswordHash = passwordHash
        };

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

        if (partitionAccesses.Any(x => x.PartitionGuid == partition.Guid))
        {
            return;
        }

        var partitionAccess = new UserPartitionAccess
        {
            User = this,
            Partition = partition
        };

        partitionAccesses.Add(partitionAccess);
    }

    public void RemovePartitionAccess(Partition partition)
    {
        var userPartition =
            partitionAccesses.SingleOrDefault(x => x == partition);

        if (userPartition == null)
        {
            return;
        }

        partitionAccesses.Remove(userPartition);
    }

    /// <summary>
    /// Resets the password expiration date based on the user's
    /// <see cref="DefaultPasswordExpirationPeriod"/>.
    ///
    /// The new expiration date is calculated by adding the configured
    /// default expiration interval to the current UTC time.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately.
    /// </summary>
    /// <remarks>
    /// This method is typically used after the user successfully changes
    /// their own password.
    /// </remarks>
    public void ResetPasswordExpiration()
        => RequirePasswordChangeAt = DateTimeOffset.UtcNow + DefaultPasswordExpirationPeriod;

    /// <summary>
    /// Marks the user's password as requiring an immediate change.
    /// </summary>
    /// <remarks>
    /// After calling this method, <see cref="IsPasswordChangeRequired"/> will return <c>true</c>
    /// until the password is updated and a new expiration date is set.
    /// </remarks>
    public void MarkPasswordChangeAsRequired()
    {
        RequirePasswordChangeAt = DateTimeOffset.UtcNow;
    }

    public void RotateAuthVersion()
    {
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>
    /// Adds a permission to the user if it does not already exist.
    /// </summary>
    /// <param name="action">The action type to allow.</param>
    public void AddPermission(ActionType action)
    {
        if (permissions.Any(x => x.Action == action))
        {
            return;
        }

        var userPermission = new UserPermission
        {
            Action = action,
            User = this
        };

        permissions.Add(userPermission);
    }

    /// <summary>
    /// Removes a permission from the user if it exists.
    /// </summary>
    /// <param name="action">The action type to remove.</param>
    public void RemovePermission(ActionType action)
    {
        var userPermission = permissions.SingleOrDefault(x => x.Action == action);

        if (userPermission == null)
        {
            return;
        }

        permissions.Remove(userPermission);
    }
}
