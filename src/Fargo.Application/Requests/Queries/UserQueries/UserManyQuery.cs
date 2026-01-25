using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserManyQuery(
        DateTime? TemporalAsOf = null,
        Pagination Pagination = default
        ) : IQuery<Task<IEnumerable<UserReadModel>>>;

    public sealed class UserManyQueryHandler(IUserReadRepository repository) : IQueryHandler<UserManyQuery, Task<IEnumerable<UserReadModel>>>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<IEnumerable<UserReadModel>> Handle(UserManyQuery query, CancellationToken cancellationToken = default)
            => await repository.GetManyAsync(
                query.TemporalAsOf,
                query.Pagination,
                cancellationToken);
    }
}