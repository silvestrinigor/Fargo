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
        ///
        /// An active group may be used normally by the application,
        /// subject to any additional business or authorization rules.
        ///
        /// An inactive group is considered disabled and may be ignored
        /// or blocked by application policies.
        /// </summary>
        /// <remarks>
        /// This property represents the current activation status of the group.
        /// Use <see cref="Activate"/> and <see cref="Deactivate"/> to change
        /// the state in a controlled way.
        /// </remarks>
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
        ///
        /// Each permission represents an allowed <see cref="ActionType"/>
        /// that members of the group may perform.
        /// </summary>
        public IReadOnlyCollection<UserGroupPermission> UserGroupPermissions
        {
            get => userGroupPermissions;
            init => userGroupPermissions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store user group permissions.
        /// </summary>
        private readonly List<UserGroupPermission> userGroupPermissions = [];

        /// <summary>
        /// Gets the read-only collection of partitions to which the user group belongs.
        /// </summary>
        /// <remarks>
        /// These partitions define the access scope of the user group.
        /// A user may access the group only if they have access to at least
        /// one of these partitions.
        /// </remarks>
        public IReadOnlyCollection<Partition> Partitions
        {
            get => partitions;
            init => partitions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store the partitions
        /// associated with the user group.
        /// </summary>
        private readonly List<Partition> partitions = [];

        /// <summary>
        /// Adds a permission to the user group if it does not already exist.
        /// </summary>
        /// <param name="action">The action type to allow.</param>
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
        /// <param name="action">The action type to remove.</param>
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
        /// <param name="partition">The partition to associate with the user group.</param>
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
        /// Removes the specified partition from the user group if it exists.
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
        /// Determines whether the user group has the specified permission.
        /// </summary>
        /// <param name="action">The action type to check.</param>
        /// <returns>
        /// <c>true</c> if the group has the permission; otherwise <c>false</c>.
        /// </returns>
        public bool HasPermission(ActionType action)
            => UserGroupPermissions.Any(p => p.Action == action);
    }
}