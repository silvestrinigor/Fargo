using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    /// <summary>
    /// Represents the read model of a user used in query operations.
    /// </summary>
    /// <remarks>
    /// This model is part of the query side (CQRS) and is used to transfer
    /// user data from the persistence layer to the application layer.
    /// </remarks>
    public class UserReadModel
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
        /// Gets the login identifier of the user.
        /// </summary>
        public required Nameid Nameid
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the description of the user.
        /// </summary>
        public required Description Description
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the hashed password stored for the user.
        /// </summary>
        public required PasswordHash PasswordHash
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the permissions assigned to the user.
        /// </summary>
        public required IReadOnlyCollection<UserPermissionReadModel> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        private readonly List<UserPermissionReadModel> userPermissions = [];
    }
}