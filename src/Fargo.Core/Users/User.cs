using Fargo.Core.Activables;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Users;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity, IPartitioned, IPartitionUser, IPartitionedGuids, IPermissionUser, IActivable
{
    private User()
    {
    }

    public User(Nameid nameid, PasswordHash passwordHash)
    {
        Nameid = nameid;
        PasswordHash = passwordHash;
    }

    public static User CreateUser(Nameid nameid, PasswordHash passwordHash)
        => new(nameid, passwordHash);

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
    /// Gets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the hashed password of the user.
    ///
    /// The raw password is never stored. Instead, a secure hash
    /// is persisted using the application's password hashing strategy.
    /// </summary>
    public PasswordHash PasswordHash { get; set; }

    public TimeSpan? DefaultPasswordExpirationPeriod { get; set; } = null;

    public DateTimeOffset? RequirePasswordChangeAt { get; set; } = null;

    public Guid AuthVersion { get; private set; } = Guid.NewGuid();

    public bool IsPasswordChangeRequired
        => RequirePasswordChangeAt is not null && DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

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
    /// Sets the password expiration requirement to a future date based on the specified number of days.
    /// </summary>
    /// <param name="days">
    /// The number of days from the current UTC time after which the user must change their password.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="days"/> is less than zero.
    /// </exception>
    public void RequirePasswordChangeInDays(int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);

        RequirePasswordChangeAt = DateTimeOffset.UtcNow.AddDays(days);
    }

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

    public void SetDefaultPasswordExpirationPeriod(TimeSpan? expirationPeriod)
    {
        if (DefaultPasswordExpirationPeriod == expirationPeriod)
        {
            return;
        }

        DefaultPasswordExpirationPeriod = expirationPeriod;
    }

    public void RotateAuthVersion()
    {
        AuthVersion = Guid.NewGuid();
    }

    /// <summary>
    /// Gets the read-only collection of permissions assigned directly to the user.
    ///
    /// Each permission represents an allowed <see cref="ActionType"/>
    /// that the user can perform without considering group memberships.
    /// </summary>
    public IReadOnlyCollection<UserPermission> Permissions
    {
        get => permissions;
        init => permissions = [.. value];
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPermission> IPermissionUser.Permissions => Permissions;

    private readonly List<UserPermission> permissions = [];

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

    /// <summary>
    /// Gets the collection of groups the user belongs to.
    /// </summary>
    /// <remarks>
    /// User groups provide additional permissions and partition access
    /// that are inherited by the user.
    ///
    /// Effective authorization for a user is typically the combination of:
    /// - Direct permissions and partition access
    /// - Permissions and partition access inherited from groups
    /// </remarks>
    public UserGroupCollection UserGroups { get; init; } = [];

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

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionAccess> IPartitionUser.PartitionAccesses => PartitionAccesses;

    private readonly List<UserPartitionAccess> partitionAccesses = [];

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

    /// <summary>
    /// Removes access to the specified partition from the user.
    /// </summary>
    /// <param name="partitionGuid">The partition identifier.</param>
    public void RemovePartitionAccess(Guid partitionGuid)
    {
        var userPartition =
            partitionAccesses.SingleOrDefault(x => x.PartitionGuid == partitionGuid);

        if (userPartition == null)
        {
            return;
        }

        partitionAccesses.Remove(userPartition);
    }

    /// <summary>
    /// Gets the partitions associated with the user entity.
    /// </summary>
    public PartitionCollection Partitions { get; init; } = [];

    IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. Partitions.Select(p => p.Guid)];

    public void AddPartition(Partition partition)
    {
        Partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        Partitions.Remove(partition);
    }

    public void AddUserGroup(UserGroup userGroup)
    {
        UserGroups.Add(userGroup);
    }

    public void RemoveUserGroup(UserGroup userGroup)
    {
        UserGroups.Remove(userGroup);
    }
}
