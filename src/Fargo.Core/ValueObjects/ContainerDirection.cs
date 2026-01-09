using Fargo.Domain.Enums;

namespace Fargo.Domain.ValueObjects
{
    public sealed record ContainerDirection(
        Guid? ContainerGuid,
        ContainerDirectionType DirectionType
        );
}
