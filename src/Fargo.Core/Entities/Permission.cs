using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class Permission : IEntityTemporal
    {
        internal Permission() { }

        public Guid UserGuid
        {
            get;
            private init;
        }

        public required User User
        {
            get;
            init
            {
                UserGuid = value.Guid;
                field = value;
            }
        }

        public required ActionType ActionType
        {
            get;
            init;
        }

        public GrantType GrantType
        {
            get;
            internal set;
        } = GrantType.Denied;
    }
}
