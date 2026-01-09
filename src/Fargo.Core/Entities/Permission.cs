using Fargo.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Domain.Entities
{
    public class Permission
    {
        internal Permission() { }

        [SetsRequiredMembers]
        internal Permission(User user, ActionType actionType, GrantType grantType) 
        {
            User = user;
            ActionType = actionType;
            GrantType = grantType;
        }

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
