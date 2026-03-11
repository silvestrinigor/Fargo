using Fargo.Application.Extensions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserGroupQueries
{
    /// <summary>
    /// Query used to retrieve a single user group by its unique identifier.
    /// </summary>
    /// <param name="UserGroupGuid">
    /// The unique identifier of the user group.
    /// </param>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the user group
    /// as it existed at the specified date and time.
    /// </param>
    public sealed record UserGroupSingleQuery(
            Guid UserGroupGuid,
            DateTimeOffset? TemporalAsOf = null
            ) : IQuery<UserGroupResponseModel?>;

    /// <summary>
    /// Handles the execution of <see cref="UserGroupSingleQuery"/>.
    /// </summary>
    public sealed class UserGroupSingleQueryHandler(
            IUserGroupQueries userGroupRepository
            ) : IQueryHandler<UserGroupSingleQuery, UserGroupResponseModel?>
    {
        /// <summary>
        /// Executes the query to retrieve a single user group.
        /// </summary>
        /// <param name="query">The query containing the user group identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The <see cref="UserGroupResponseModel"/> if the user group exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<UserGroupResponseModel?> Handle(
                UserGroupSingleQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var userGroup = await userGroupRepository.GetByGuid(
                    query.UserGroupGuid,
                    query.TemporalAsOf,
                    cancellationToken
                    );

            return userGroup?.ToResponse();
        }
    }
}