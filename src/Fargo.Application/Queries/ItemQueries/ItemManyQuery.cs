using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ItemQueries;

public sealed record ItemManyQuery(
    Guid? ArticleGuid = null,
    DateTimeOffset? TemporalAsOf = null,
    Pagination? Pagination = null
) : IQuery<IReadOnlyCollection<ItemInformation>>;

public sealed class ItemManyQueryHandler(
        IItemRepository itemRepository
        ) : IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>>
{
    public async Task<IReadOnlyCollection<ItemInformation>> Handle(
            ItemManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        var items = await itemRepository.GetManyInfo(
                query.Pagination ?? Pagination.FirstPage20Items,
                query.ArticleGuid,
                query.TemporalAsOf,
                cancellationToken
                );

        return items;
    }
}
