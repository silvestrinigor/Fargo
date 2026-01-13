using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserPermissionAllQuery(
        Guid UserGuid,
        DateTime? TemporalAsOf = null
        ) : IQuery<IEnumerable<PermissionReadModel>?>;

    public sealed class UserPermissionAllQueryHandler(IUserReadRepository repository) : IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<PermissionReadModel>?>
    {
        private readonly IUserReadRepository repository = repository;

        public async Task<IEnumerable<PermissionReadModel>?> HandleAsync(UserPermissionAllQuery query, CancellationToken cancellationToken = default)
            => await repository.GetUserPermissions(
                query.UserGuid, 
                query.TemporalAsOf, 
                cancellationToken);
    }
}