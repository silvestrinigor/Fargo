using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Articles;

internal static class ArticleManagerLog
{
    private static readonly Action<ILogger, Guid, string, Exception?> _articleUpdateFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Error, default, "Failed to update article {ArticleGuid}: {Detail}");

    internal static void LogArticleUpdateFailed(this ILogger logger, Guid articleGuid, string detail) =>
        _articleUpdateFailed(logger, articleGuid, detail, null);
}
