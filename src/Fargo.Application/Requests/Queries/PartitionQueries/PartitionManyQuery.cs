using Fargo.Application.Commom;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionManyQuery(
        DateTime? TemporalAsOf = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<PartitionReadModel>>;

    public sealed class PartitionManyQueryHandler(IPartitionReadRepository repository) : IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionReadModel>>
    {
        public readonly IPartitionReadRepository repository = repository;

        public async Task<IEnumerable<PartitionReadModel>> HandleAsync(PartitionManyQuery query, CancellationToken cancellationToken = default)
            => await repository.GetManyAsync(
                query.TemporalAsOf,
                query.Pagination,
                cancellationToken);
    }
}
