using Fargo.Application.Dtos;
using Fargo.Domain.Entities.Events.Abstracts;
using System.Text.Json;

namespace Fargo.Application.Extensions
{
    public static class EventExtension
    {
        extension(Event @event)
        {
            public EventDto ToDto()
                => new(
                    Guid: @event.Guid,
                    OccurredAt: @event.OccurredAt,
                    ModelGuid: @event.ModelGuid,
                    ModelType: @event.ModelType,
                    EventType: @event.EventType
                    );
        }
    }
}
