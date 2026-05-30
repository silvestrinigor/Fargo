using Fargo.Application.Identity;
using Fargo.Core.Shared.Barcodes;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

#region Guid

/// <summary>
/// Query used to retrieve an article by identifier.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="AsOfDateTime">
/// Temporal query date.
/// </param>
public sealed record ArticleByGuidQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

/// <summary>
/// Handles article queries by identifier.
/// </summary>
/// <remarks>
/// Retrieves an article visible to the current actor.
/// </remarks>
public sealed class ArticleByGuidQueryHandler(
    IArticleQueryRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleByGuidQueryHandler> logger
) : IQueryHandler<ArticleByGuidQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleByGuidQuery query,
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
            childOfAnyOfThesePartitions: actor.PartitionAccesses,
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

#endregion Guid

#region Barcode

/// <summary>
/// Query used to retrieve an article by barcode.
/// </summary>
/// <param name="ArticleBarcode">
/// Article barcode information.
/// </param>
/// <param name="AsOfDateTime">
/// Temporal query date.
/// </param>
public sealed record ArticleByBarcodeQuery(
    Barcode ArticleBarcode,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

/// <summary>
/// Handles article queries by barcode.
/// </summary>
/// <remarks>
/// Retrieves an article using barcode information.
/// </remarks>
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
                query.ArticleBarcode.Format,
                actor.ActorGuid);
        }

        var article = await articleRepository.GetInfoByBarcode(
            query.ArticleBarcode,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query completed for barcode type {BarcodeType} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleBarcode.Format,
                actor.ActorGuid,
                article is not null);
        }

        return article;
    }
}

#endregion Barcode

#region Many

/// <summary>
/// Query used to retrieve multiple articles.
/// </summary>
/// <param name="WithPagination">
/// Pagination configuration.
/// </param>
/// <param name="TemporalAsOfDateTime">
/// Temporal query date.
/// </param>
/// <param name="ChildOfAnyOfThesePartitions">
/// Filters articles inside the provided partitions.
/// </param>
/// <param name="NotChildOfAnyPartition">
/// Indicates whether articles without partitions should be included.
/// </param>
public sealed record ArticlesQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;

/// <summary>
/// Handles queries for multiple articles.
/// </summary>
/// <remarks>
/// Retrieves paginated article collections visible
/// to the current actor.
/// </remarks>
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
