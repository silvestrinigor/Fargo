using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system.
    /// </summary>
    public class User
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
        /// Gets or sets the description of the user.
        /// </summary>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        internal PasswordHash PasswordHash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the permissions of the user.
        /// </summary>
        public IReadOnlyCollection<UserPermission> Permissions => permissions;

        private readonly List<UserPermission> permissions = [];

        public void AddPermission(ActionType action)
        {
            if (permissions.Any(p => p.ActionType == action))
                return;

            var permissionToAdd = new UserPermission
            {
                User = this,
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

        public PartitionCollection Partitions
        {
            get;
            init;
        } = [];
    }
}