using Fargo.Domain.Enums;

namespace Fargo.Domain.ValueObjects.Entities
{
    public sealed record Permission(
            Guid Guid,
            ActionType Action
            );
}