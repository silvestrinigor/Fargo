using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ArticleQueries;

/// <summary>
/// Query used to retrieve a single article information projection
/// accessible to the current user.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the article
/// as it existed at the specified date and time.
/// </param>
public sealed record ArticleSingleQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleInformation?>;

/// <summary>
/// Handles the execution of <see cref="ArticleSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// the requested article only if it belongs to at least one of those partitions.
///
/// If the article does not exist or is not accessible to the current user,
/// <see langword="null"/> is returned.
/// </remarks>
public sealed class ArticleSingleQueryHandler(
    IArticleRepository articleRepository,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<ArticleSingleQuery, ArticleInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single article information projection
    /// accessible to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the article identifier and optional as-of date.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="ArticleInformation"/> accessible to the current user,
    /// or <see langword="null"/> if the article does not exist or is not accessible.
    /// </returns>
    public async Task<ArticleInformation?> Handle(
        ArticleSingleQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var partitionAccessGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken
        );

        var article = await articleRepository.GetInfoByGuidInPartitions(
            query.ArticleGuid,
            partitionAccessGuids,
            query.AsOfDateTime,
            cancellationToken
        );

        return article;
    }
}
