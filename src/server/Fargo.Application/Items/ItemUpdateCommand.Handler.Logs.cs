using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item update flow started for item '{itemGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void UpdateStarted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType
    );

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item update mutation completed for item '{itemGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void UpdateCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType
    );
}
