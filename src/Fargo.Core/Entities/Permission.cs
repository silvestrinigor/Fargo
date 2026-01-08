using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class Permission
    {
        internal Permission() { }

        public required Guid UserGuid { get; init; }

        public required ActionType PermissionType { get; init; }

        public GrantType GrantType 
        { 
            get; 
            set; 
        } = GrantType.Denied;
    }
}
