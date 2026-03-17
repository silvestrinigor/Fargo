using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.UserGroupQueries
{
        public sealed record UserGroupSingleQuery(
                Guid UserGroupGuid,
                DateTimeOffset? TemporalAsOf = null
                ) : IQuery<UserGroupInformation?>;

        public sealed class UserGroupSingleQueryHandler(
                IUserGroupRepository userGroupRepository
                ) : IQueryHandler<UserGroupSingleQuery, UserGroupInformation?>
        {
                public async Task<UserGroupInformation?> Handle(
                        UserGroupSingleQuery query,
                        CancellationToken cancellationToken = default
                        )
                {
                        var userGroup = await userGroupRepository.GetInfoByGuid(
                                query.UserGroupGuid,
                                query.TemporalAsOf,
                                cancellationToken
                                );

                        return userGroup;
                }
        }
}