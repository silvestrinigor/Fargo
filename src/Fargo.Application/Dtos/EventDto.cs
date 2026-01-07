using Fargo.Domain.Enums;

namespace Fargo.Application.Dtos
{
    public sealed record EventDto(
        Guid Guid,
        DateTime OccurredAt,
        Guid ModelGuid,
        EventType EventType
        );
}
