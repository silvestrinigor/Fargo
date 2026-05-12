using Fargo.Application.Authentication;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

#region Single

public sealed record ArticleSingleQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

public sealed class ArticleSingleQueryHandler(
    IArticleQueryRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSingleQueryHandler> logger
) : IQueryHandler<ArticleSingleQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article single query started for article {ArticleGuid} by actor {ActorGuid}.",
                query.ArticleGuid,
                actor.ActorGuid);
        }

        var article = await articleRepository.GetInfoByGuid(
            query.ArticleGuid,
            query.AsOfDateTime,
            actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article single query completed for article {ArticleGuid} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleGuid,
                actor.ActorGuid,
                article is not null);
        }

        return article;
    }
}

#endregion Single

#region Barcode

public sealed record ArticleByBarcodeQuery(
    ArticleBarcodeDto ArticleBarcode,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

public sealed class ArticleByBarcodeQueryHandler(
    IArticleQueryRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleByBarcodeQueryHandler> logger
) : IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleByBarcodeQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query started for barcode type {BarcodeType} by actor {ActorGuid}.",
                query.ArticleBarcode.Type,
                actor.ActorGuid);
        }

        var article = await articleRepository.GetInfoByBarcode(
            query.ArticleBarcode,
            query.AsOfDateTime,
            actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query completed for barcode type {BarcodeType} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleBarcode.Type,
                actor.ActorGuid,
                article is not null);
        }

        return article;
    }
}

#endregion Barcode

#region Many

public sealed record ArticlesQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;

public sealed class ArticlesQueryHandler(
    IArticleQueryRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticlesQueryHandler> logger
) : IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>
{
    public async Task<IReadOnlyCollection<ArticleDto>> Handle(
        ArticlesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Articles query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actor.ActorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccesses,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var articles = await articleRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Articles query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.ActorGuid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                articles.Count);
        }

        return articles;
    }
}

#endregion Many
