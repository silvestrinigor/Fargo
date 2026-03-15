using Fargo.Domain.Collections;
using Fargo.Domain.Enums;
using Fargo.Domain.Logics;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user group in the system.
    /// </summary>
    /// <remarks>
    /// A user group contains descriptive information and a collection of permissions
    /// that define which actions members of the group are allowed to perform.
    ///
    /// A user group is partitioned data and may belong to multiple
    /// <see cref="Partition"/> instances.
    ///
    /// A user may access the group only if they have access to at least one of the
    /// partitions associated with it, subject to additional authorization rules.
    /// </remarks>
    public class UserGroup : AuditedEntity, IPartitioned, IPartitionUser, IUserWithPermissions
    {
        /// <summary>
        /// Gets or sets the unique NAMEID of the user group.
        /// </summary>
        /// <remarks>
        /// This value uniquely identifies the group in the system and must satisfy
        /// the validation rules defined by <see cref="Nameid"/>.
        /// </remarks>
        public required Nameid Nameid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the textual description associated with the user group.
        /// </summary>
        /// <remarks>
        /// This field provides additional contextual information about the purpose
        /// of the group. If not specified, it defaults to <see cref="Description.Empty"/>.
        /// </remarks>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user group is active.
        /// </summary>
        /// <remarks>
        /// Inactive groups should not be considered available for permission grants
        /// or operational use, depending on application rules.
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
        /// Gets the read-only collection of permissions assigned to the user group.
        /// </summary>
        /// <remarks>
        /// This collection represents the permissions granted by the group.
        /// It is part of the group authorization model and is used to determine
        /// which actions members of the group are allowed to perform.
        /// </remarks>
        public IReadOnlyCollection<UserGroupPermission> Permissions
        {
            get => userGroupPermissions;
            init => userGroupPermissions = [.. value];
        }

        IReadOnlyCollection<IPermission> IUserWithPermissions.Permissions => Permissions;

        private readonly List<UserGroupPermission> userGroupPermissions = [];

        /// <summary>
        /// Adds a permission to the user group if it does not already exist.
        /// </summary>
        /// <param name="action">The action to grant to the user group.</param>
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
        /// <param name="action">The action to remove from the user group.</param>
        public void RemovePermission(ActionType action)
        {
            var userGroupPermission = userGroupPermissions.SingleOrDefault(x => x.Action == action);

            if (userGroupPermission == null)
            {
                return;
            }

            userGroupPermissions.Remove(userGroupPermission);
        }

        public PartitionCollection Partitions
        {
            get;
            init;
        }

        IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;

        /// <summary>
        /// Gets the read-only collection of users associated with the user group.
        /// </summary>
        /// <remarks>
        /// This collection represents users that belong to the group.
        /// It is primarily used for relationship navigation and persistence mapping.
        /// Domain operations related to membership should be controlled through
        /// explicit behaviors rather than by directly mutating this collection.
        /// </remarks>
        public IReadOnlyCollection<User> Users
        {
            get => users;
            init => users = [.. value];
        }

        private readonly List<User> users = [];

        public IReadOnlyCollection<UserGroupPartitionAccess> PartitionsAccesses
        {
            get => partitionAccesses;
            init => partitionAccesses = [.. value];
        }

        public IReadOnlyCollection<IPartitionAccess> PartitionAccesses => PartitionAccesses;

        private List<UserGroupPartitionAccess> partitionAccesses = [];

        public void AddPartitionAccess(Partition partition)
        {
            ArgumentNullException.ThrowIfNull(partition);

            if (partitionAccesses.Any(x => x == partition))
            {
                return;
            }

            var partitionAccess = new UserGroupPartitionAccess
            {
                UserGroup = this,
                Partition = partition
            };

            partitionAccesses.Add(partitionAccess);
        }

        public void RemovePartitionAccess(Guid partitionGuid)
        {
            var userGroupPartition =
                partitionAccesses.SingleOrDefault(x => x.PartitionGuid == partitionGuid);

            if (userGroupPartition == null)
            {
                return;
            }

            partitionAccesses.Remove(userGroupPartition);
        }
    }
}