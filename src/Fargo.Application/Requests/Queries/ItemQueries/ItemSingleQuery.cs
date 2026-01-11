using Fargo.Application.Mediators;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTime? AtDateTime = null
        ) : IQuery<ItemReadModel?>;

    public sealed class ItemSingleQueryHandler(IItemReadRepository repository) : IQueryHandlerAsync<ItemSingleQuery, ItemReadModel?>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<ItemReadModel?> HandleAsync(ItemSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.ItemGuid, 
                query.AtDateTime, 
                cancellationToken);
    }
}
