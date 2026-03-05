using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a user entity in the system.
    /// </summary>
    public class User : Entity
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

        public IReadOnlyCollection<UserPermission> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        private readonly List<UserPermission> userPermissions = [];

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

        public void RemovePermission(ActionType action)
        {
            var userPermission = userPermissions.SingleOrDefault(x => x.Action == action);

            if (userPermission == null)
            {
                return;
            }

            userPermissions.Remove(userPermission);
        }

        public bool HasPermission(ActionType action)
            => UserPermissions.Any(p => p.Action == action);

        public void ValidatePermission(ActionType action)
        {
            if (!HasPermission(action))
            {
                throw new UserNotAuthorizedException(Guid, action);
            }
        }
    }
}