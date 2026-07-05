using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Barcodes;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

internal static partial class ArticleByBarcodeQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article query by barcode flow started for article {articleBarcode} by actor {actorId}.")]
    public static partial void QueryByBarcodeStarted(
        this ILogger logger,
        Barcode articleBarcode,
        ActorId actorId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Article query by barcode flow completed for article {articleBarcode} by actor {actorId}.")]
    public static partial void QueryByBarcodeCompleted(
        this ILogger logger,
        Barcode articleBarcode,
        ActorId actorId);
}
