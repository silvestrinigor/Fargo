using Fargo.Application.Dtos;
using Fargo.Application.Dtos.EventDataDtos;
using Fargo.Domain.Entities.Events;
using Fargo.Domain.Entities.Events.Abstracts;
using System.Text.Json;

namespace Fargo.Application.Extensions
{
    public static class EventExtension
    {
        extension(Event @event)
        {
            public EventDto ToDto()
            {
                JsonElement? eventData = @event switch
                {
                    ArticleCreatedEvent e 
                    => JsonSerializer.SerializeToElement(
                        new ArticleCreatedEventDataDto(e.ArticleName)
                        ),
                    
                    ItemCreatedEvent e
                    => JsonSerializer.SerializeToElement(
                        new ItemCreatedEventDataDto(e.ItemArticleGuid)
                        ),

                    _
                    => null
                };

                return new(
                    Guid: @event.Guid,
                    OccurredAt: @event.OccurredAt,
                    ModelGuid: @event.ModelGuid,
                    EventType: @event.EventType,
                    EventData: eventData
                    );
            }
        }
    }
}
