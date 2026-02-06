using Fargo.Application.Commom;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionManyQuery(
            DateTime? TemporalAsOf = null,
            Pagination Pagination = default
            ) : IQuery<Task<IEnumerable<PartitionReadModel>>>;

    public sealed class PartitionManyQueryHandler(
            IPartitionReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<PartitionManyQuery, Task<IEnumerable<PartitionReadModel>>>
    {
        private readonly IPartitionReadRepository repository = repository;

        private readonly ICurrentUser currentUser = currentUser;

        public async Task<IEnumerable<PartitionReadModel>> Handle(
                PartitionManyQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetManyAsync(
                    currentUser.PartitionGuids,
                    query.TemporalAsOf,
                    query.Pagination,
                    cancellationToken
                    );
    }
}