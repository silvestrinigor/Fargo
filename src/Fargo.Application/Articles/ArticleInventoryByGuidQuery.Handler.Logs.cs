using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleInventoryByGuidQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article inventory query by guid started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void QueryInventoryByGuidStarted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article inventory query by guid completed for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'. Found: {found}. Count: {count}.")]
    public static partial void QueryInventoryByGuidCompleted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType, bool found, int count);
}
