using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserGroupQueries;

public sealed record UserGroupManyQuery(
        Guid? UserGuid = null,
        DateTimeOffset? TemporalAsOf = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<UserGroupInformation>>;

public sealed class UserGroupManyQueryHandler(
        IUserGroupRepository userGroupRepository
        ) : IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupInformation>>
{
    public async Task<IReadOnlyCollection<UserGroupInformation>> Handle(
            UserGroupManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        var userGroups = await userGroupRepository.GetManyInfo(
                query.Pagination ?? Pagination.FirstPage20Items,
                query.UserGuid,
                query.TemporalAsOf,
                cancellationToken
                );

        return userGroups;
    }
}
