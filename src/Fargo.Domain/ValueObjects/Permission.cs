using Fargo.Domain.Enums;

namespace Fargo.Domain.ValueObjects
{
    public readonly struct Permission
    {
        public required ActionType ActionType
        {
            get;
            init;
        }

        public required GrantType GrantType
        {
            get;
            init;
        }
    }
}