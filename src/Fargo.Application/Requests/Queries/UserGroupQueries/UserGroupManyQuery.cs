using Fargo.Application.Common;
using Fargo.Application.Extensions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserGroupQueries
{
    public sealed record UserGroupManyQuery(
            DateTimeOffset? TemporalAsOf = null,
            Pagination? Pagination = null
            ) : IQuery<IReadOnlyCollection<UserGroupResponseModel>>;

    public sealed class UserGroupManyQueryHandler(
            IUserQueries userRepository
            ) : IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupResponseModel>>
    {
        public async Task<IReadOnlyCollection<UserGroupResponseModel>> Handle(
                UserGroupManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            throw new NotImplementedException();
        }
    }
}