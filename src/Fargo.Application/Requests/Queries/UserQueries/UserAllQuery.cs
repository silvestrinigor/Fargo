using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories.UserRepositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserAllQuery(
        DateTime? AtDateTime,
        Pagination Pagination
        ) : IQuery<IEnumerable<UserDto>>;

    public sealed class UserAllQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserAllQuery, IEnumerable<UserDto>>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<IEnumerable<UserDto>> HandleAsync(UserAllQuery query, CancellationToken cancellationToken = default)
        {
            var users = await repository.GetManyAsync(query.AtDateTime, query.Pagination, cancellationToken);

            return users.Select(x => x.ToDto());
        }
    }
}
