using Fargo.Domain.Enums;
using System.Text.Json;

namespace Fargo.Application.Dtos
{
    public sealed record EventDto(
        Guid Guid,
        DateTime OccurredAt,
        Guid ModelGuid,
        EventType EventType,
        JsonElement? EventData = null
        );
}
