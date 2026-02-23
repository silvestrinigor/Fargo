using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system with permissions and partition management.
    /// </summary>
    public class User
    {
        internal User() { }

        /// <summary>
        /// Gets the unique identifier (GUID) for the user.
        /// </summary>
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        /// <summary>
        /// Gets the numeric identifier for the user.
        /// </summary>
        public int Id
        {
            get;
            init;
        } = 0;

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public required Name Name
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

        /// <summary>
        /// Gets the collection of permissions associated with the user.
        /// </summary>
        public IReadOnlyCollection<UserPermission> Permissions => permissions;
        
        private readonly List<UserPermission> permissions = [];

        public void SetPermission(ActionType actionType, GrantType grantType)
        {
            var permission = permissions.SingleOrDefault(x => x.ActionType == actionType);

            if (permission is not null)
            {
                permission.GrantType = grantType;

                return;
            }

            permissions.Add(
                new UserPermission
                {
                    User = this,
                    ActionType = actionType,
                    GrantType = grantType
                });
        }

        public bool HasPermission(ActionType actionType)
        {
            var permission = permissions.SingleOrDefault(x => x.ActionType == actionType);

            return permission is not null && permission.GrantType == GrantType.Granted;
        }

        private readonly HashSet<Partition> partitions = [];

        public IReadOnlyCollection<Partition> Partitions => partitions;

        internal PasswordHash PasswordHash
        {
            get;
            set;
        }
    }
}