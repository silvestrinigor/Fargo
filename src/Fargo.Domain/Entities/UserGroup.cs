using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user group in the system.
    ///
    /// A user group contains descriptive information and a collection
    /// of permissions that define which actions members of the group
    /// are allowed to perform.
    /// </summary>
    /// <remarks>
    /// A user group is partitioned data and may belong to multiple
    /// <see cref="Partition"/> instances.
    ///
    /// A user may access the group only if they have access to at least
    /// one of the partitions associated with it, subject to additional
    /// authorization rules.
    /// </remarks>
    public class UserGroup : AuditedEntity, IPartitioned
    {
        /// <summary>
        /// Gets or sets the unique NAMEID of the user group.
        /// This value uniquely identifies the group in the system.
        /// </summary>
        public required Nameid Nameid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the textual description associated with the user group.
        /// If not specified, the description defaults to <see cref="Description.Empty"/>.
        /// </summary>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets a value indicating whether the user group is active.
        /// </summary>
        public bool IsActive
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Marks the user group as active.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Marks the user group as inactive.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Validates whether the user group is active.
        /// </summary>
        /// <exception cref="UserGroupInactiveFargoDomainException">
        /// Thrown when the user group is inactive.
        /// </exception>
        public void ValidateIsActive()
        {
            if (!IsActive)
            {
                throw new UserGroupInactiveFargoDomainException(Guid);
            }
        }

        /// <summary>
        /// Gets the read-only collection of permissions assigned to the user group.
        /// </summary>
        public IReadOnlyCollection<UserGroupPermission> UserGroupPermissions
        {
            get => userGroupPermissions;
            init => userGroupPermissions = [.. value];
        }

        private readonly List<UserGroupPermission> userGroupPermissions = [];

        /// <summary>
        /// Gets the read-only collection of partitions to which the user group belongs.
        /// </summary>
        public IReadOnlyCollection<Partition> Partitions
        {
            get => partitions;
            init => partitions = [.. value];
        }

        private readonly List<Partition> partitions = [];

        /// <summary>
        /// Adds a permission to the user group if it does not already exist.
        /// </summary>
        public void AddPermission(ActionType action)
        {
            if (userGroupPermissions.Any(x => x.Action == action))
            {
                return;
            }

            var userGroupPermission = new UserGroupPermission
            {
                Action = action,
                UserGroup = this
            };

            userGroupPermissions.Add(userGroupPermission);
        }

        /// <summary>
        /// Removes a permission from the user group if it exists.
        /// </summary>
        public void RemovePermission(ActionType action)
        {
            var userGroupPermission = userGroupPermissions.SingleOrDefault(x => x.Action == action);

            if (userGroupPermission == null)
            {
                return;
            }

            userGroupPermissions.Remove(userGroupPermission);
        }

        /// <summary>
        /// Adds the specified partition to the user group if it is not already associated.
        /// </summary>
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
        /// Removes the specified partition from the user group if it exists.
        /// </summary>
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
        /// Determines whether the user group has the specified permission.
        /// </summary>
        public bool HasPermission(ActionType action)
            => userGroupPermissions.Any(p => p.Action == action);

        /// <summary>
        /// Determines whether the specified user can access this user group.
        /// </summary>
        /// <param name="user">The user to evaluate.</param>
        /// <returns>
        /// <see langword="true"/> if the group has no partitions or the user has
        /// access to at least one of the group's partitions; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="user"/> is <see langword="null"/>.
        /// </exception>
        public bool CanBeAccessedBy(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (!Partitions.Any())
            {
                return true;
            }

            return Partitions.Any(user.HasPartitionAccess);
        }

        /// <summary>
        /// Determines whether the specified user can use this group to perform the given action.
        /// </summary>
        /// <param name="user">The user to evaluate.</param>
        /// <param name="action">The action to check.</param>
        /// <returns>
        /// <see langword="true"/> if the group is active, has the specified permission,
        /// and the user can access the group by partition; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="user"/> is <see langword="null"/>.
        /// </exception>
        public bool GrantsPermissionTo(User user, ActionType action)
        {
            ArgumentNullException.ThrowIfNull(user);

            return IsActive
                && HasPermission(action)
                && CanBeAccessedBy(user);
        }
    }
}