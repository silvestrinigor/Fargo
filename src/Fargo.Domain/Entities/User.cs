using Fargo.Domain.Collections;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
/// <remarks>
/// A user contains authentication credentials, direct permissions,
/// partition access, and group memberships that may grant additional
/// permissions and access.
///
/// Authorization for a user is determined by the combination of:
/// - Direct permissions and partition access
/// - Permissions and partition access inherited from user groups
/// </remarks>
public class User : ModifiedEntity, IPartitioned, IPartitionUser, IPermissionUser
{
    /// <summary>
    /// Gets or sets the unique NAMEID (username) of the user.
    /// This value identifies the user in the system and must satisfy
    /// the validation rules defined by <see cref="Nameid"/>.
    /// </summary>
    public required Nameid Nameid
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the user's first name.
    ///
    /// This value is optional and, when provided, must satisfy the
    /// validation rules defined by <see cref="ValueObjects.FirstName"/>.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the first name
    /// has not been specified.
    /// </remarks>
    public FirstName? FirstName
    {
        get;
        set;
    } = null;

    /// <summary>
    /// Gets or sets the user's last name.
    ///
    /// This value is optional and, when provided, must satisfy the
    /// validation rules defined by <see cref="ValueObjects.LastName"/>.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value indicates that the last name
    /// has not been specified.
    /// </remarks>
    public LastName? LastName
    {
        get;
        set;
    } = null;

    /// <summary>
    /// Gets or sets the textual description associated with the user.
    /// If not specified, the description defaults to <see cref="Description.Empty"/>.
    /// </summary>
    public Description Description
    {
        get;
        set;
    } = Description.Empty;

    /// <summary>
    /// Gets or sets the hashed password of the user.
    ///
    /// The raw password is never stored. Instead, a secure hash
    /// is persisted using the application's password hashing strategy.
    /// </summary>
    public required PasswordHash PasswordHash
    {
        get;
        set;
    }

    /// <summary>
    /// The default number of days a user can keep the same password
    /// before a password change is required.
    /// </summary>
    public const int DefaultPasswordChangeDays = 90;

    /// <summary>
    /// Gets or sets the default password expiration period for the user.
    ///
    /// This value is persisted and represents the amount of time added to the
    /// current UTC time when the user successfully changes their own password.
    ///
    /// A value of <see cref="TimeSpan.Zero"/> causes the password to expire
    /// immediately after being changed.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the assigned value is less than <see cref="TimeSpan.Zero"/>.
    /// </exception>
    public TimeSpan DefaultPasswordExpirationPeriod
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
            field = value;
        }
    } = TimeSpan.FromDays(DefaultPasswordChangeDays);

    /// <summary>
    /// Gets or sets the date and time when the user must change their password.
    ///
    /// This value is initialized at entity creation time based on
    /// <see cref="DefaultPasswordChangeDays"/> from the current UTC time.
    /// </summary>
    public DateTimeOffset RequirePasswordChangeAt
    {
        get;
        set;
    } = DateTimeOffset.UtcNow + TimeSpan.FromDays(DefaultPasswordChangeDays);

    /// <summary>
    /// Determines whether the user is currently required to change their password.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the current UTC time is greater than or equal to
    /// <see cref="RequirePasswordChangeAt"/>; otherwise, <c>false</c>.
    /// </returns>
    public bool IsPasswordChangeRequired
        => DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

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

    /// <summary>
    /// Gets a value indicating whether the user is active.
    ///
    /// An active user is allowed to authenticate and interact with the system,
    /// subject to any additional authorization or security rules.
    ///
    /// An inactive user is considered disabled and may be prevented from
    /// signing in or performing operations, depending on application policies.
    /// </summary>
    /// <remarks>
    /// This property represents the user's current activation status.
    /// </remarks>
    public bool IsActive
    {
        get;
        set;
    } = true;

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
    public UserGroupCollection UserGroups
    {
        get;
        init;
    } = [];

    /// <summary>
    /// Gets the read-only collection of partitions the user has access to.
    /// </summary>
    /// <remarks>
    /// Partitions define logical boundaries in the system.
    /// A user can only access entities that belong to at least one partition
    /// to which the user has been granted access.
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
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
    /// <remarks>
    /// These partitions define the partition scope of the user entity itself,
    /// not the partitions the user has access to.
    ///
    /// This is used for partition-based access control on the user entity,
    /// meaning a user can only access this user record if they have access
    /// to at least one of these partitions.
    ///
    /// To determine which partitions the user can access, see
    /// <see cref="PartitionAccesses"/> and <see cref="UserGroups"/>.
    /// </remarks>
    public PartitionCollection Partitions
    {
        get;
        init;
    } = [];

    /// <inheritdoc />
    IReadOnlyCollection<Partition> IPartitioned.Partitions => Partitions;
}
