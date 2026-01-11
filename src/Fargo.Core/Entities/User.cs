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

            permissions.Add(new Permission(
                this,
                actionType,
                grantType
                ));
        }

        public bool HasPermission(ActionType actionType)
        {
            return permissions.Where(x => x.ActionType == actionType).SingleOrDefault()?.GrantType == GrantType.Granted;
        }
    }
}
