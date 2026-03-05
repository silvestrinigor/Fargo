using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class UserPermission
    {
        public Guid UserGuid
        {
            get;
            private set;
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

        public required ActionType Action
        {
            get;
            init;
        }
    }
}