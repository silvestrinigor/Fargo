using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserSingleQuery(
        Guid UserGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<UserReadModel?>;

    public sealed class UserSingleQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserSingleQuery, UserReadModel?>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<UserReadModel?> HandleAsync(UserSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.UserGuid, 
                query.TemporalAsOf, 
                cancellationToken);
    }
}
