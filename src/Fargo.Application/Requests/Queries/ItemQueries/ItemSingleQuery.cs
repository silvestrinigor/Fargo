using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<ItemReadModel?>;

    public sealed class ItemSingleQueryHandler(
            IItemReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<ItemSingleQuery, ItemReadModel?>
    {
        public async Task<ItemReadModel?> Handle(
                ItemSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                query.ItemGuid,
                currentUser.PartitionGuids,
                query.TemporalAsOf,
                cancellationToken
                );
    }
}