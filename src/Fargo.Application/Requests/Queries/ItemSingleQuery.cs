using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ItemSingleQuery(Guid ItemGuid) : IQuery<ItemDto>;

    public sealed class ItemSingleQueryHandler(IItemRepository repository) : IQueryHandlerAsync<ItemSingleQuery, ItemDto>
    {
        public async Task<ItemDto> HandleAsync(ItemSingleQuery query, CancellationToken cancellationToken = default)
        {
            var item = await repository.GetByGuidAsync(query.ItemGuid, cancellationToken)
                ?? throw new InvalidOperationException("Item not found.");

            return item.ToDto();
        }
    }
}
