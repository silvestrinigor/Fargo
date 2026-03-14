using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserGroupModels
{
    /// <summary>
    /// Represents the read model of a user group used in query operations.
    /// </summary>
    /// <remarks>
    /// This model belongs to the query side of the application (CQRS) and is used
    /// to transfer user group data from the persistence layer to the application layer
    /// without exposing domain entities directly.
    /// </remarks>
    public class UserGroupReadModel : AuditedEntityReadModel
    {
        /// <summary>
        /// Gets the login identifier (NAMEID) of the user group.
        /// </summary>
        public required Nameid Nameid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the description associated with the user group.
        /// </summary>
        public required Description Description
        {
            get;
            init;
        }

        /// <summary>
        /// Gets a value indicating whether the user group is active.
        /// </summary>
        public required bool IsActive
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the permissions assigned to the user group.
        /// </summary>
        /// <remarks>
        /// Each permission defines an action that the user group is allowed to perform.
        /// </remarks>
        public required IReadOnlyCollection<UserGroupPermissionReadModel> UserGroupPermissions
        {
            get => userGroupPermissions;
            init => userGroupPermissions = [.. value];
        }

        private readonly List<UserGroupPermissionReadModel> userGroupPermissions = [];
    }
}