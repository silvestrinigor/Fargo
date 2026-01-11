using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Mediators;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories.UserRepositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserPermissionAllQuery(
        Guid UserGuid,
        DateTime? AtDateTime
        ) : IQuery<IEnumerable<UserPermissionDto>>;

    public sealed class UserPermissionAllQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<UserPermissionDto>>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<IEnumerable<UserPermissionDto>> HandleAsync(UserPermissionAllQuery query, CancellationToken cancellationToken = default)
        {
            var user = await repository.GetByGuidAsync(query.UserGuid, query.AtDateTime, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            return user.Permissions.Select(x => new UserPermissionDto(x.ActionType, GrantType.Granted));
        }
    }
}
