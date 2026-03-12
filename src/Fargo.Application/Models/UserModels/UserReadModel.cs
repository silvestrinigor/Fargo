using Fargo.Domain.ValueObjects;
using Fargo.Application.Models.UserGroupModels;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the read model of a user used in query operations.
    /// </summary>
    /// <remarks>
    /// This model belongs to the query side of the application (CQRS) and is used
    /// to transfer user data from the persistence layer to the application layer
    /// without exposing domain entities directly.
    /// </remarks>
    public class UserReadModel : AuditedEntityReadModel
    {
        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        public required Guid Guid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the login identifier (NAMEID) of the user.
        /// </summary>
        public required Nameid Nameid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the first name of the user.
        /// </summary>
        /// <remarks>
        /// This value may be <c>null</c> when the user's first name is not specified.
        /// </remarks>
        public required FirstName? FirstName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the last name of the user.
        /// </summary>
        /// <remarks>
        /// This value may be <c>null</c> when the user's last name is not specified.
        /// </remarks>
        public required LastName? LastName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the default password expiration period configured for the user.
        /// </summary>
        /// <remarks>
        /// This value represents the duration added to the current time when the
        /// user successfully changes their password.
        /// </remarks>
        public required TimeSpan DefaultPasswordExpirationPeriod
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the date and time when the user must change their password.
        /// </summary>
        /// <remarks>
        /// When the current time is greater than or equal to this value,
        /// the user is required to change their password.
        /// </remarks>
        public required DateTimeOffset RequirePasswordChangeAt
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the description associated with the user.
        /// </summary>
        public required Description Description
        {
            get;
            init;
        }

        /// <summary>
        /// Gets a value indicating whether the user is active.
        /// </summary>
        public required bool IsActive
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the permissions assigned directly to the user.
        /// </summary>
        /// <remarks>
        /// Each permission defines an action that the user is allowed to perform.
        /// </remarks>
        public required IReadOnlyCollection<UserPermissionReadModel> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        private readonly List<UserPermissionReadModel> userPermissions = [];

        /// <summary>
        /// Gets the groups associated with the user.
        /// </summary>
        /// <remarks>
        /// Each group may grant additional permissions to the user.
        /// </remarks>
        public required IReadOnlyCollection<UserGroupReadModel> UserGroups
        {
            get => userGroups;
            init => userGroups = [.. value];
        }

        private readonly List<UserGroupReadModel> userGroups = [];
    }
}