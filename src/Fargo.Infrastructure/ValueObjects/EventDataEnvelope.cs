using Fargo.Domain.ValueObjects.EventsValueObjects;
using Fargo.Infrastructure.Enums;
using System.Text.Json;

namespace Fargo.Infrastructure.ValueObjects
{
    public record EventDataEnvelope(
        EventDataType EventDataType,
        JsonElement EventData
        );
}
