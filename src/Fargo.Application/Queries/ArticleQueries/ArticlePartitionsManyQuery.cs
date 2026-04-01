using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ArticleQueries;

public sealed record ArticlePartitionsManyQuery(
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<PartitionInformation>>;

public sealed class ArticlePartitionsManyQueryHandler(
        ActorService actorService,
        IArticleRepository articleRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<ArticlePartitionsManyQuery, IReadOnlyCollection<PartitionInformation>>
{
    public async Task<IReadOnlyCollection<PartitionInformation>> Handle(
            ArticlePartitionsManyQuery query,
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
