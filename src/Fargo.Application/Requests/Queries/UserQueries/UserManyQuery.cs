using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserManyQuery(
            DateTime? TemporalAsOf = null,
            Pagination Pagination = default
            ) : IQuery<IEnumerable<UserReadModel>>;

    public sealed class UserManyQueryHandler(
            IUserReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<UserManyQuery, IEnumerable<UserReadModel>>
    {
        public async Task<IEnumerable<UserReadModel>> Handle(
                UserManyQuery query,
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