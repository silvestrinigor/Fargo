using Fargo.Domain.Enums;

namespace Fargo.Application.Dtos
{
    public sealed record EventDto(
        Guid Guid,
        Guid RelatedEntityGuid,
        DateTime OccurredAt,
        EventType EventType,
        object? EventData
        );
}
