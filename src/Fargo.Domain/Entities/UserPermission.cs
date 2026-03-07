using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a permission assigned to a user.
    ///
    /// Each instance defines that a specific <see cref="User"/> is allowed
    /// to perform a particular <see cref="ActionType"/>.
    /// </summary>
    public class UserPermission
    {
        /// <summary>
        /// Gets the unique identifier of the user that owns this permission.
        /// </summary>
        public Guid UserGuid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the user associated with this permission.
        ///
        /// When the user is assigned, the <see cref="UserGuid"/> is automatically
        /// synchronized with the user's identifier.
        /// </summary>
        public required User User
        {
            get;
            init
            {
                UserGuid = value.Guid;
                field = value;
            }
        }

        /// <summary>
        /// Gets the action that the user is allowed to perform.
        /// </summary>
        public required ActionType Action
        {
            get;
            init;
        }
    }
}