using Fargo.Domain.Enums;
using System.Text.Json;

namespace Fargo.Application.Dtos
{
    public sealed record EventDto(
        Guid Guid,
        Guid RelatedEntityGuid,
        DateTime OccurredAt,
        EventType EventType,
        JsonElement? EventData
        );
}
