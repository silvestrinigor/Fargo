using Fargo.Core.Shared.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item create flow started for article {articleGuid} by actor {actorId}.")]
    public static partial void CreateStarted(
        this ILogger logger, Guid articleGuid, ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item create mutation completed for item {itemGuid} by actor {actorId}. ArticleGuid: {articleGuid}.")]
    public static partial void CreateCompleted(
        this ILogger logger, Guid itemGuid, ActorId actorId, Guid articleGuid);
}
