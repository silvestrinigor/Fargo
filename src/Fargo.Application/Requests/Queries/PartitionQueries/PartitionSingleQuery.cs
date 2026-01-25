using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionSingleQuery(
        Guid PartitionGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<Task<PartitionReadModel?>>;

    public sealed class PartitionSingleQueryHandler(IPartitionReadRepository repository) : IQueryHandler<PartitionSingleQuery, Task<PartitionReadModel?>>
    {
        private readonly IPartitionReadRepository repository = repository;

        public async Task<PartitionReadModel?> Handle(PartitionSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.PartitionGuid,
                query.TemporalAsOf,
                cancellationToken);
    }
}