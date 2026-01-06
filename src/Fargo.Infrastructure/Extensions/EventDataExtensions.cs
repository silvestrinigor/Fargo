using Fargo.Domain.ValueObjects.EventsValueObjects;
using Fargo.Infrastructure.Enums;
using Fargo.Infrastructure.ValueObjects;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

namespace Fargo.Infrastructure.Extensions
{
    public static class EventDataExtensions
    {
        extension(IEventData eventData)
        {
            public EventDataEnvelope ToEventDataEnvelope()
            {
                return eventData switch
                {
                    ArticleCreatedEventData x 
                    => new EventDataEnvelope(
                        EventDataType.ArticleCreatedEventData,
                        JsonSerializer.SerializeToElement(x)
                        ),

                    ItemCreatedEventData x 
                    => new EventDataEnvelope(
                        EventDataType.ItemCreatedEventData,
                        JsonSerializer.SerializeToElement(x)
                        ),

                    _ => throw new InvalidOperationException("Unknown event data type")
                };
            }
        }

        extension(EventDataEnvelope eventDataEnvelope)
        {
            public IEventData ToEventData()
            {
                return eventDataEnvelope.EventDataType switch
                {
                    EventDataType.ArticleCreatedEventData
                    => JsonSerializer.Deserialize<ArticleCreatedEventData>(
                        eventDataEnvelope.EventData
                        ) ?? throw new InvalidOperationException(),

                    EventDataType.ItemCreatedEventData
                    => JsonSerializer.Deserialize<ItemCreatedEventData>(
                        eventDataEnvelope.EventData
                        ) ?? throw new InvalidOperationException(),

                    _ => throw new InvalidOperationException("Unknown event data type")
                };
            }
        }
    }
}
