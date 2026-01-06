using Fargo.Application.Dtos;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects.EventsValueObjects;
using System.Text.Json;

namespace Fargo.Application.Extensions
{
    public static class EventExtension
    {
        extension(Event @event)
        {
            public EventDto ToDto()
            {
                return @event.EventType switch
                {
                    EventType.ArticleCreated =>
                    new EventDto(
                        Guid: @event.Guid,
                        RelatedEntityGuid: @event.EntityGuid,
                        OccurredAt: @event.OccurredAt,
                        EventType: @event.EventType,
                        EventData: 
                            JsonSerializer.SerializeToElement(
                                @event.EventData as ArticleCreatedEventData
                                )
                        ),

                    EventType.ItemCreated =>
                    new EventDto(
                        Guid: @event.Guid,
                        RelatedEntityGuid: @event.EntityGuid,
                        OccurredAt: @event.OccurredAt,
                        EventType: @event.EventType,
                        EventData:
                            JsonSerializer.SerializeToElement(
                                @event.EventData as ItemCreatedEventData
                                )
                        ),

                    _ => 
                    new EventDto(
                        Guid: @event.Guid,
                        RelatedEntityGuid: @event.EntityGuid,
                        OccurredAt: @event.OccurredAt,
                        EventType: @event.EventType,
                        EventData: null
                        )
                };
            }
        }
    }
}
