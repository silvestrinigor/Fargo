using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item update flow started for item {itemGuid} by actor {actorId}.")]
    public static partial void UpdateStarted(
        this ILogger logger, Guid itemGuid, ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item update mutation completed for item {itemGuid} by actor {actorId}.")]
    public static partial void UpdateCompleted(
        this ILogger logger, Guid itemGuid, ActorId actorId);
}
