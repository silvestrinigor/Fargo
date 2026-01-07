using Fargo.Application.Dtos;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;

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
                    OccurredAt: @event.OccurredAt
                    );
            }
        }
    }
}
