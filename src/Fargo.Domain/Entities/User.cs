using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system.
    ///
    /// A user contains authentication credentials, direct permissions,
    /// and group memberships that may also grant permissions.
    /// </summary>
    public class User : AuditedEntity, IPartitioned
    {
        /// <summary>
        /// Gets or sets the unique NAMEID (username) of the user.
        /// This value uniquely identifies the user in the system.
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
        /// By default, this value is initialized based on
        /// <see cref="DefaultPasswordChangeDays"/> from the current UTC time.
        /// </summary>
        public DateTimeOffset RequirePasswordChangeAt
        {
            get;
            set;
        } = DateTimeOffset.UtcNow + TimeSpan.FromDays(DefaultPasswordChangeDays);

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
        /// Determines whether the user is currently required to change their password.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current UTC time is greater than or equal to
        /// <see cref="RequirePasswordChangeAt"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPasswordChangeRequired
            => DateTimeOffset.UtcNow >= RequirePasswordChangeAt;

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
        /// Use <see cref="Activate"/> and <see cref="Deactivate"/> to change the state
        /// in a controlled way.
        /// </remarks>
        public bool IsActive
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Marks the user as active.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Marks the user as inactive.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Validates whether the user is active.
        /// </summary>
        /// <exception cref="UserInactiveFargoDomainException">
        /// Thrown when the user is inactive.
        /// </exception>
        public void ValidateIsActive()
        {
            if (!IsActive)
            {
                throw new UserInactiveFargoDomainException(Guid);
            }
        }

        /// <summary>
        /// Gets the read-only collection of permissions assigned directly to the user.
        ///
        /// Each permission represents an allowed <see cref="ActionType"/>
        /// that the user can perform without considering group memberships.
        /// </summary>
        public IReadOnlyCollection<UserPermission> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store direct user permissions.
        /// </summary>
        private readonly List<UserPermission> userPermissions = [];

        /// <summary>
        /// Gets the read-only collection of groups to which the user belongs.
        /// </summary>
        /// <remarks>
        /// Group memberships may grant additional permissions to the user.
        /// </remarks>
        public IReadOnlyCollection<UserGroup> UserGroups
        {
            get => userGroups;
            init => userGroups = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store user group memberships.
        /// </summary>
        private readonly List<UserGroup> userGroups = [];

        /// <summary>
        /// Gets the read-only collection of partitions the user has access to.
        /// </summary>
        /// <remarks>
        /// Partitions define logical boundaries in the system.
        /// A user can only access entities that belong to at least one partition
        /// to which the user has been granted access.
        /// </remarks>
        public IReadOnlyCollection<PartitionAccess> PartitionsAccesses
        {
            get => partitionAccesses;
            init => partitionAccesses = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store the partitions
        /// the user has access to.
        /// </summary>
        private readonly List<PartitionAccess> partitionAccesses = [];

        /// <summary>
        /// Adds a permission to the user if it does not already exist.
        /// </summary>
        /// <param name="action">The action type to allow.</param>
        public void AddPermission(ActionType action)
        {
            if (userPermissions.Any(x => x.Action == action))
            {
                return;
            }

            var userPermission = new UserPermission
            {
                Action = action,
                User = this
            };

            userPermissions.Add(userPermission);
        }

        /// <summary>
        /// Removes a permission from the user if it exists.
        /// </summary>
        /// <param name="action">The action type to remove.</param>
        public void RemovePermission(ActionType action)
        {
            var userPermission = userPermissions.SingleOrDefault(x => x.Action == action);

            if (userPermission == null)
            {
                return;
            }

            userPermissions.Remove(userPermission);
        }

        /// <summary>
        /// Adds the specified group to the user if it is not already associated.
        /// </summary>
        /// <param name="userGroup">The group to associate with the user.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="userGroup"/> is <see langword="null"/>.
        /// </exception>
        public void AddGroup(UserGroup userGroup)
        {
            ArgumentNullException.ThrowIfNull(userGroup);

            if (userGroups.Any(x => x.Guid == userGroup.Guid))
            {
                return;
            }

            userGroups.Add(userGroup);
        }

        /// <summary>
        /// Removes the specified group from the user if it exists.
        /// </summary>
        /// <param name="userGroupGuid">The unique identifier of the group to remove.</param>
        public void RemoveGroup(Guid userGroupGuid)
        {
            var userGroup = userGroups.SingleOrDefault(x => x.Guid == userGroupGuid);

            if (userGroup == null)
            {
                return;
            }

            userGroups.Remove(userGroup);
        }

/// <summary>
/// Determines whether the user has the specified permission,
/// either directly or through at least one active and accessible group membership.
/// </summary>
/// <param name="action">The action type to check.</param>
/// <returns>
/// <c>true</c> if the user has the permission directly or through
/// at least one active group that the user can access by partition;
/// otherwise <c>false</c>.
/// </returns>
public bool HasPermission(ActionType action)
    => userPermissions.Any(p => p.Action == action)
    || userGroups.Any(g => g.GrantsPermissionTo(this, action));

        /// <summary>
        /// Validates whether the user has the specified permission.
        /// </summary>
        /// <param name="action">The action that must be authorized.</param>
        /// <exception cref="UserNotAuthorizedFargoDomainException">
        /// Thrown when the user does not have the required permission.
        /// </exception>
        public void ValidatePermission(ActionType action)
        {
            if (!HasPermission(action))
            {
                throw new UserNotAuthorizedFargoDomainException(Guid, action);
            }
        }

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

            var partitionAccess = new PartitionAccess
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
        /// Gets the read-only collection of partitions to which the user belongs.
        /// </summary>
        /// <remarks>
        /// These partitions define the data scope of the user itself.
        /// Other users may only access this user if they have access to at least
        /// one of these partitions.
        /// </remarks>
        public IReadOnlyCollection<Partition> Partitions
        {
            get => partitions;
            init => partitions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store the partitions
        /// to which the user belongs.
        /// </summary>
        private readonly List<Partition> partitions = [];

        /// <summary>
        /// Adds the specified partition to the user if it is not already associated.
        /// </summary>
        /// <param name="partition">The partition to associate with the user.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
        /// </exception>
        public void AddPartition(Partition partition)
        {
            ArgumentNullException.ThrowIfNull(partition);

            if (partitions.Any(x => x.Guid == partition.Guid))
            {
                return;
            }

            partitions.Add(partition);
        }

        /// <summary>
        /// Removes the specified partition from the user if it exists.
        /// </summary>
        /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
        public void RemovePartition(Guid partitionGuid)
        {
            var partition = partitions.SingleOrDefault(x => x.Guid == partitionGuid);

            if (partition == null)
            {
                return;
            }

            partitions.Remove(partition);
        }

/// <summary>
/// Determines whether the user has access to the specified partition.
/// </summary>
/// <param name="partition">The partition to evaluate.</param>
/// <returns>
/// <c>true</c> if the user has explicit access to the partition; otherwise <c>false</c>.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="partition"/> is <see langword="null"/>.
/// </exception>
public bool HasPartitionAccess(Partition partition)
{
    ArgumentNullException.ThrowIfNull(partition);

    return partitionAccesses.Any(x => x.PartitionGuid == partition.Guid);
}

        /// <summary>
        /// Determines whether the user has access to the specified partitioned entity.
        /// </summary>
        /// <param name="partitioned">
        /// The partitioned entity to evaluate.
        /// </param>
        /// <returns>
        /// <c>true</c> if the entity belongs to at least one partition the user has access to,
        /// or if the entity is not associated with any partition; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="partitioned"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Access is granted when:
        /// <list type="bullet">
        /// <item>
        /// The entity has no partitions associated with it, making it accessible to all users.
        /// </item>
        /// <item>
        /// There is at least one intersection between the partitions associated with the
        /// entity and the partitions the user has access to.
        /// </item>
        /// </list>
        /// </remarks>
        public bool HasAccess(IPartitioned partitioned)
        {
            ArgumentNullException.ThrowIfNull(partitioned);

            if (!partitioned.Partitions.Any())
            {
                return true;
            }

            return partitioned.Partitions.Any(partition =>
                    partitionAccesses.Any(access => access.PartitionGuid == partition.Guid));
        }
    }
}