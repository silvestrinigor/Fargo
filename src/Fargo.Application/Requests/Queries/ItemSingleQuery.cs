using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTime? AtDateTime
        ) : IQuery<ItemDto?>;

    public sealed class ItemSingleQueryHandler(IItemReadRepository repository) : IQueryHandlerAsync<ItemSingleQuery, ItemDto?>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<ItemDto?> HandleAsync(ItemSingleQuery query, CancellationToken cancellationToken = default)
        {
            var item = await repository.GetByGuidAsync(query.ItemGuid, query.AtDateTime, cancellationToken);

            return item?.ToDto();
        }
    }
}
