using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories.UserRepositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserSingleQuery(
        Guid UserGuid,
        DateTime? AtDateTime = null
        ) : IQuery<UserDto?>;

    public sealed class UserSingleQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserSingleQuery, UserDto?>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<UserDto?> HandleAsync(UserSingleQuery query, CancellationToken cancellationToken = default)
        {
            var user = await repository.GetByGuidAsync(query.UserGuid, query.AtDateTime, cancellationToken);

            return user?.ToDto();
        }
    }
}
