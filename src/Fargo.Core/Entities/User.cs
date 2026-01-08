using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class User
    {
        internal User() : base() { }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public required Name Name 
        { 
            get; 
            init; 
        }

        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        public required PasswordHash PasswordHash
        { 
            get;
            init;
        }

        private readonly HashSet<Permission> permissions = [];

        public void SetPermission(ActionType permissionType, GrantType grantType)
        {
            var permission = permissions.Where(x => x.PermissionType == permissionType).SingleOrDefault();

            if (permission is not null)
            {
                permission.GrantType = grantType;
                return;
            }

            if (permission is null)
            {
                permission = new Permission
                {
                    UserGuid = this.Guid,
                    PermissionType = permissionType,
                    GrantType = grantType
                };

                permissions.Add(permission);
            }
        }
    }
}
