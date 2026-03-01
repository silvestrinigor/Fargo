using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user group that can be assigned permissions and associated with users.
    /// </summary>
    public class UserGroup : AuditedEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier (NAMEID) of the user.
        /// </summary>
        public required Nameid Name
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

        public HashSet<ActionType> Permissions
        {
            get;
            init;
        } = [];

        public HashSet<User> Users
        {
            get;
            init;
        } = [];

        public HashSet<Partition> Partitions
        {
            get;
            init;
        } = [];

        public HashSet<Partition> PartitionsAccesses
        {
            get;
            init;
        } = [];
    }
}