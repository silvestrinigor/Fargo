using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class User : IEntityByGuid, IEntityTemporal
    {
        public User() : base() { }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public required int Id
        {
            get;
            init;
        };

        public required Name Name
        {
            get;
            set;
        }

        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        internal PasswordHash PasswordHash
        {
            get;
            set;
        }

        public IReadOnlyCollection<Permission> Permissions => permissions;

        private readonly HashSet<Permission> permissions = [];

        public void SetPermission(ActionType actionType, GrantType grantType)
        {
            var permission = permissions.Where(x => x.ActionType == actionType).SingleOrDefault();

            if (permission is not null)
            {
                permission.GrantType = grantType;

                return;
            }

            permissions.Add(
                new Permission
                {
                    User = this,
                    ActionType = actionType,
                    GrantType = grantType
                });
        }

        public bool HasPermission(ActionType actionType)
        {
            var permission = permissions.Where(x => x.ActionType == actionType).SingleOrDefault();

            return permission is not null && permission.GrantType == GrantType.Granted;
        }
    }
}
