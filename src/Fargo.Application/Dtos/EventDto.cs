using Fargo.Domain.Enums;

namespace Fargo.Application.Dtos
{
    public sealed record EventDto(
        Guid Guid,
        Guid EntityGuid,
        DateTime OccurredAt,
        EventType EventType
        );
}
