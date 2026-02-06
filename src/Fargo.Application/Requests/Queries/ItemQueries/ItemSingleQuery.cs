using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<Task<ItemReadModel?>>;

    public sealed class ItemSingleQueryHandler(
            IItemReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<ItemSingleQuery, Task<ItemReadModel?>>
    {
        private readonly IItemReadRepository repository = repository;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task<ItemReadModel?> Handle(
                ItemSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuidAsync(
                query.ItemGuid,
                currentUser.PartitionGuids,
                query.TemporalAsOf,
                cancellationToken
                );
    }
}