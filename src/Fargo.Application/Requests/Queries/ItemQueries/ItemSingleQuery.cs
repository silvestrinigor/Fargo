using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<Task<ItemReadModel?>>;

    public sealed class ItemSingleQueryHandler(IItemReadRepository repository) : IQueryHandler<ItemSingleQuery, Task<ItemReadModel?>>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<ItemReadModel?> Handle(ItemSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.ItemGuid,
                query.TemporalAsOf,
                cancellationToken);
    }
}