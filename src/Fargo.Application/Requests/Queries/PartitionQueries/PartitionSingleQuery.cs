using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionSingleQuery(
        Guid PartitionGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<PartitionReadModel?>;

    public sealed class PartitionSingleQueryHandler(IPartitionReadRepository repository) : IQueryHandlerAsync<PartitionSingleQuery, PartitionReadModel?>
    {
        private readonly IPartitionReadRepository repository = repository;

        public async Task<PartitionReadModel?> HandleAsync(PartitionSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.PartitionGuid,
                query.TemporalAsOf,
                cancellationToken);
    }
}
