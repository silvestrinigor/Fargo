using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.UserModels
{
    public class UserReadModel
    {
        public required Guid Guid
        {
            get;
            init;
        }

        public required Nameid Nameid
        {
            get;
            init;
        }

        public required Description Description
        {
            get;
            init;
        }

        public required PasswordHash PasswordHash
        {
            get;
            init;
        }

        public required IReadOnlyCollection<UserPermissionReadModel> UserPermissions
        {
            get => userPermissions;
            init => userPermissions = [.. value];
        }

        private readonly List<UserPermissionReadModel> userPermissions = [];
    }
}