using Fargo.Application.Identity;
using Fargo.Application.Shared.Articles;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Queries;

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
