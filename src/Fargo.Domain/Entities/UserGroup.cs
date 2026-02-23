using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities;

/// <summary>
/// Represents a user group that can be assigned permissions and associated with users.
/// </summary>
public class UserGroup
{
        internal UserGroup() { }

        /// <summary>
        /// Gets the unique identifier (GUID) for the user group.
        /// </summary>
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the user group.
        /// </summary>
        public required Name Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the user group.
        /// </summary>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets the collection of permissions associated with the user group.
        /// </summary>
        public IReadOnlyCollection<UserPermission> Permissions => permissions;
        private readonly List<UserPermission> permissions = [];

        /// <summary>
        /// Gets the collection of users that are members of this user group.
        /// </summary>
        public IReadOnlyCollection<User> Users => users;
        private readonly List<User> users = [];
}
