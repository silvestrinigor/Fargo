using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class MoveItemEventCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item movement event flow started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void MoveItemEventStarted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item movement event '{itemEventGuid}' recorded for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void MoveItemEventCompleted(
        this ILogger logger, Guid itemEventGuid, Guid articleGuid, Guid actorGuid, ActorType actorType);
}
