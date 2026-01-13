using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserManyQuery(
        DateTime? TemporalAsOf = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<UserReadModel>>;

    public sealed class UserManyQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserManyQuery, IEnumerable<UserReadModel>>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<IEnumerable<UserReadModel>> HandleAsync(UserManyQuery query, CancellationToken cancellationToken = default)
            => await repository.GetManyAsync(
                query.TemporalAsOf, 
                query.Pagination, 
                cancellationToken);
    }
}
