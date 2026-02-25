using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a permission assigned to a user for a specific action type.
    /// </summary>
    public class UserPermission
    {
        internal UserPermission() { }

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
    }
}