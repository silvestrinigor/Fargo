using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system.
    ///
    /// A user contains authentication credentials and a collection
    /// of permissions that define which actions they are allowed to perform.
    /// </summary>
    public class User : Entity
    {
        /// <summary>
        /// Gets or sets the unique NAMEID (username) of the user.
        /// This value uniquely identifies the user in the system.
        /// </summary>
        public required Nameid Nameid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the textual description associated with the user.
        /// If not specified, the description defaults to <see cref="Description.Empty"/>.
        /// </summary>
        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        /// <summary>
        /// Gets or sets the hashed password of the user.
        ///
        /// The raw password is never stored. Instead, a secure hash
        /// is persisted using the application's password hashing strategy.
        /// </summary>
        public required PasswordHash PasswordHash
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the collection of permissions assigned to the user.
        ///
        /// Each permission represents an allowed <see cref="ActionType"/>
        /// that the user can perform.
        /// </summary>
        public IReadOnlyCollection<UserPermission> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        /// <summary>
        /// Internal mutable collection used to store user permissions.
        /// </summary>
        private readonly List<UserPermission> userPermissions = [];

        /// <summary>
        /// Adds a permission to the user if it does not already exist.
        /// </summary>
        /// <param name="action">The action type to allow.</param>
        public void AddPermission(ActionType action)
        {
            if (userPermissions.Any(x => x.Action == action))
            {
                return;
            }

            var userPermission = new UserPermission
            {
                Action = action,
                User = this
            };

            userPermissions.Add(userPermission);
        }

        /// <summary>
        /// Removes a permission from the user if it exists.
        /// </summary>
        /// <param name="action">The action type to remove.</param>
        public void RemovePermission(ActionType action)
        {
            var userPermission = userPermissions.SingleOrDefault(x => x.Action == action);

            if (userPermission == null)
            {
                return;
            }

            userPermissions.Remove(userPermission);
        }

        /// <summary>
        /// Determines whether the user has the specified permission.
        /// </summary>
        /// <param name="action">The action type to check.</param>
        /// <returns>
        /// <c>true</c> if the user has the permission; otherwise <c>false</c>.
        /// </returns>
        public bool HasPermission(ActionType action)
            => UserPermissions.Any(p => p.Action == action);

        /// <summary>
        /// Validates whether the user has the specified permission.
        /// </summary>
        /// <param name="action">The action that must be authorized.</param>
        /// <exception cref="UserNotAuthorizedFargoDomainException">
        /// Thrown when the user does not have the required permission.
        /// </exception>
        public void ValidatePermission(ActionType action)
        {
            if (!HasPermission(action))
            {
                throw new UserNotAuthorizedFargoDomainException(Guid, action);
            }
        }
    }
}