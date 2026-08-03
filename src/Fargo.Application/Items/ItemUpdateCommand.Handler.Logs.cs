using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemUpdateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item update flow started for item {itemGuid} by actor {actorGuid}.")]
    public static partial void UpdateStarted(
        this ILogger logger, Guid itemGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item update mutation completed for item {itemGuid} by actor {actorGuid}.")]
    public static partial void UpdateCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid);
}
