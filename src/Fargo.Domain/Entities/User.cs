using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system.
    /// </summary>
    public class User : AuditedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier (NAMEID) of the user.
        /// </summary>
        public required Nameid Nameid
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

        public required PasswordHash PasswordHash
        {
            get;
            set;
        }

        public HashSet<ActionType> Permissions
        {
            get;
            init;
        } = [];

        public HashSet<Partition> PartitionsAccesses
        {
            get;
            init;
        } = [];

        public HashSet<Partition> Partitions
        {
            get;
            init;
        } = [];

        public bool HasPermission(ActionType action)
            => Permissions.Any(p => p == action);

        public void ValidatePermission(ActionType action)
        {
            if (!HasPermission(action))
            {
                throw new UserNotAuthorizedException(Guid, action);
            }
        }
    }
}