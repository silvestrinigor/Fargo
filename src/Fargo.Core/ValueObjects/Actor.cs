using Fargo.Domain.Enums;

namespace Fargo.Domain.ValueObjects
{
    public readonly struct Actor
    {
        public required Guid UserGuid
        {
            get;
            init;
        }

        public IReadOnlySet<Permission> UserPermissions
        {
            get;
            init;
        }

        public IReadOnlySet<Guid> PartitionGuids
        {
            get;
            init;
        }

        public bool HasPermission(ActionType action)
            => UserPermissions
            .SingleOrDefault(x => x.ActionType == action)
            .GrantType == GrantType.Granted;
    }
}