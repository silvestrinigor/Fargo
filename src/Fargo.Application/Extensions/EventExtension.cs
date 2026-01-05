using Fargo.Application.Dtos;
using Fargo.Domain.Entities;

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
                    EventData: @event.EventData
                    );
            }
        }
    }
}
