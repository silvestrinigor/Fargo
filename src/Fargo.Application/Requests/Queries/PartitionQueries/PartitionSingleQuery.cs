using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionSingleQuery(
            Guid PartitionGuid,
            DateTime? TemporalAsOf = null
            ) : IQuery<Task<PartitionReadModel?>>;

    public sealed class PartitionSingleQueryHandler(
            IPartitionReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<PartitionSingleQuery, Task<PartitionReadModel?>>
    {
        private readonly IPartitionReadRepository repository = repository;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task<PartitionReadModel?> Handle(
                PartitionSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuidAsync(
                    query.PartitionGuid,
                    currentUser.PartitionGuids,
                    query.TemporalAsOf,
                    cancellationToken
                    );
    }
}