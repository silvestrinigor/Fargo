using Fargo.Core.Actors;
using Fargo.Core.Barcodes;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article query by guid started for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ArticleQueryByGuidStarted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article single query completed for article '{articleGuid}' by actor '{actorGuid}' of type '{actorType}'. Found: {found}.")]
    public static partial void ArticleQueryByGuidCompleted(
        this ILogger logger, Guid articleGuid, Guid actorGuid, ActorType actorType, bool found);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article query by barcode flow started for article '{articleBarcode}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void ArticleQueryByBarcodeStarted(
        this ILogger logger, Barcode articleBarcode, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Article query by barcode flow completed for article '{articleBarcode}' by actor '{actorGuid}' of type '{actorType}'. Found: {found}.")]
    public static partial void ArticleQueryByBarcodeCompleted(
        this ILogger logger, Barcode articleBarcode, Guid actorGuid, ActorType actorType, bool found);
}
