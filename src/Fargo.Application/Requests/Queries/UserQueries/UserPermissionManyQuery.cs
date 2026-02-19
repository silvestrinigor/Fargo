using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserPermissionManyQuery(
            Guid UserGuid,
            DateTime? TemporalAsOf = null,
            Pagination Pagination = default
            ) : IQuery<IEnumerable<PermissionReadModel>?>;

    public sealed class UserPermissionAllQueryHandler(
            IUserReadRepository repository
            ) : IQueryHandler<UserPermissionManyQuery, IEnumerable<PermissionReadModel>?>
    {
        public async Task<IEnumerable<PermissionReadModel>?> Handle(
                UserPermissionManyQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetUserPermissions(
                    query.UserGuid,
                    query.TemporalAsOf,
                    query.Pagination,
                    cancellationToken
                    );
    }
}