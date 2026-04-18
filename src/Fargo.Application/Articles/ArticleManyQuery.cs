using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Articles;

namespace Fargo.Application.Queries.ArticleQueries;

/// <summary>
/// Query used to retrieve a paginated collection of <see cref="ArticleInformation"/>
/// accessible to the current actor.
/// </summary>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned results represent the state of the articles
/// as they existed at the specified date and time.
/// </param>
/// <param name="Pagination">
/// Optional pagination configuration used to control the number of returned
/// results and the starting position of the query.
/// When <see langword="null"/>, a default pagination is applied.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only articles visible to the current actor are returned.
/// </remarks>
public sealed record ArticleManyQuery(
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<ArticleInformation>>;

/// <summary>
/// Handles <see cref="ArticleManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering articles based on partition access.</description></item>
/// <item><description>Applying optional temporal (as-of) and pagination constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access all articles.
/// </para>
///
/// <para>
/// Regular actors can only access articles that belong to at least one
/// partition they have access to.
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if an article belonged
/// to a partition that has since been deleted at the time of the request,
/// the following rules apply:
/// <list type="bullet">
/// <item>
/// <description>
/// Administrative and system actors can still access the historical data.
/// </description>
/// </item>
/// <item>
/// <description>
/// Regular actors will not have access to such articles, as the partition
/// no longer exists in the current context, and the result will be excluded
/// from the returned collection.
/// </description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public sealed class ArticleManyQueryHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleInformation>>
{
    /// <summary>
    /// Executes the query to retrieve article information accessible
    /// to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing optional temporal and pagination parameters.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="ArticleInformation"/> objects
    /// visible to the current actor.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, all articles
    /// are returned without partition filtering.
    ///
    /// Otherwise, only articles belonging to partitions accessible to the
    /// actor are returned.
    ///
    /// Pagination defaults to <see cref="Pagination.FirstPage20Items"/> when not specified.
    /// </remarks>
    public async Task<IReadOnlyCollection<ArticleInformation>> Handle(
            ArticleManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            var articles = await articleRepository.GetManyInfo(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return articles;
        }
        else
        {
            var articles = await articleRepository.GetManyInfoInPartitions(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    actor.PartitionAccesses,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return articles;
        }
    }
}
