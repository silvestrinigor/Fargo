using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Domain;

namespace Fargo.Application.Articles;

/// <summary>
/// Query used to retrieve the partitions that directly contain a specific article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article.</param>
public sealed record ArticlePartitionsQuery(Guid ArticleGuid) : IQuery<IReadOnlyCollection<PartitionInformation>?>;

/// <summary>
/// Handles <see cref="ArticlePartitionsQuery"/>.
/// </summary>
public sealed class ArticlePartitionsQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser
) : IQueryHandler<ArticlePartitionsQuery, IReadOnlyCollection<PartitionInformation>?>
{
    /// <summary>
    /// Returns the partitions that directly contain the article, filtered by the current
    /// actor's partition accesses. Admins and system actors receive all partitions unfiltered.
    /// Returns <see langword="null"/> if the article does not exist.
    /// </summary>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    public async Task<IReadOnlyCollection<PartitionInformation>?> Handle(
        ArticlePartitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var filter = (actor.IsAdmin || actor.IsSystem) ? null : actor.PartitionAccesses;

        return await articleRepository.GetPartitions(query.ArticleGuid, filter, cancellationToken);
    }
}
