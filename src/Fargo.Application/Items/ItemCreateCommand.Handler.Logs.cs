using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

internal static partial class ItemCreateCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item create flow started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void CreateStarted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Item create mutation completed for item '{itemGuid}' by actor '{actorGuid}' of type '{actorType}'. ArticleGuid: {articleGuid}.")]
    public static partial void CreateCompleted(
        this ILogger logger, Guid itemGuid, Guid actorGuid, ActorType actorType, Guid articleGuid);
}
