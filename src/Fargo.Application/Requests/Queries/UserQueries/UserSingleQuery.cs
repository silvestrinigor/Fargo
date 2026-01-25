using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserSingleQuery(
        Guid UserGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<Task<UserReadModel?>>;

    public sealed class UserSingleQueryHandler(IUserReadRepository repository) : IQueryHandler<UserSingleQuery, Task<UserReadModel?>>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<UserReadModel?> Handle(UserSingleQuery query, CancellationToken cancellationToken = default)
            => await repository.GetByGuidAsync(
                query.UserGuid,
                query.TemporalAsOf,
                cancellationToken);
    }
}