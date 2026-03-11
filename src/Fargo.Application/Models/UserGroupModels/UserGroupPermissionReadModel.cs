using Fargo.Domain.Enums;

namespace Fargo.Application.Models.UserGroupModels
{
    /// <summary>
    /// Represents the read model of a user group permission used in query operations.
    /// </summary>
    public class UserGroupPermissionReadModel
    {
        /// <summary>
        /// Gets the unique identifier of the permission entry.
        /// </summary>
        public required Guid Guid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the unique identifier of the user group who owns the permission.
        /// </summary>
        public required Guid UserGroupGuid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the action granted to the user group.
        /// </summary>
        public required ActionType Action
        {
            get;
            init;
        }
    }
}