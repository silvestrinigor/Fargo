using Fargo.Application.Dtos;
using Fargo.Domain.Entities.Events.Abstracts;

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
                    OccurredAt: @event.OccurredAt,
                    ModelGuid: @event.ModelGuid,
                    EventType: @event.EventType
                    );
            }
        }
    }
}
