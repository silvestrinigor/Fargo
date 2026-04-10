using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ArticleQueries;

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
    IArticleRepository articleRepository,
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
