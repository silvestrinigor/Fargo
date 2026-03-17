using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTimeOffset? TemporalAsOf = null
        ) : IQuery<ItemInformation?>;

    public sealed class ItemSingleQueryHandler(
            IItemRepository itemRepository
            ) : IQueryHandler<ItemSingleQuery, ItemInformation?>
    {
        public async Task<ItemInformation?> Handle(
                ItemSingleQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var i = await itemRepository.GetInfoByGuid(
                query.ItemGuid,
                query.TemporalAsOf,
                cancellationToken
                );

            return i;
        }
    }
}