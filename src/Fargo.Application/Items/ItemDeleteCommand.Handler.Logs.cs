using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemDeleteCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item delete flow started for item {itemGuid} by actor {actorGuid}.")]
    public static partial void DeleteStarted(
        this ILogger logger, Guid itemGuid, Guid actorGuid);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item delete mutation completed for item {itemGuid} by actor {actorGuid}.")]
    public static partial void DeleteCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid);
}
