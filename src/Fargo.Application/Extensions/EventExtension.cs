using Fargo.Application.Dtos;
using Fargo.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace Fargo.Application.Extensions
{
    public static class EventExtension
    {
        extension(Event @event)
        {
            public EventDto ToDto()
            {
                return new EventDto(
                    Guid: @event.Guid,
                    RelatedEntityGuid: @event.RelatedEntityGuid,
                    OccurredAt: @event.OccurredAt,
                    EventType: @event.EventType,
                    EventData: @event.EventJsonData is not null 
                    ? JsonSerializer.Deserialize<JsonElement>(@event.EventJsonData)
                    : null
                    );
            }
        }
    }
}
