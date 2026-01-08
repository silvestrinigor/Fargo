using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record UserAllQuery(
        PaginationDto Pagination
        ) : IQuery<IEnumerable<UserDto>>;

    public sealed class UserAllQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserAllQuery, IEnumerable<UserDto>>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<IEnumerable<UserDto>> HandleAsync(UserAllQuery query, CancellationToken cancellationToken = default)
        {
            var users = await repository.GetAllAsync(query.Pagination.Skip, query.Pagination.Limit, cancellationToken);

            return users.Select(x => x.ToDto());
        }
    }
}
