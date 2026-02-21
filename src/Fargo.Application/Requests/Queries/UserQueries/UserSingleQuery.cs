using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserSingleQuery(
            Guid UserGuid,
            DateTime? TemporalAsOf = null
            ) : IQuery<UserReadModel?>;

    public sealed class UserSingleQueryHandler(
            IUserReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<UserSingleQuery, UserReadModel?>
    {
        public async Task<UserReadModel?> Handle(
                UserSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                    query.UserGuid,
                    currentUser.PartitionGuids,
                    query.TemporalAsOf,
                    cancellationToken
                    );
    }
}