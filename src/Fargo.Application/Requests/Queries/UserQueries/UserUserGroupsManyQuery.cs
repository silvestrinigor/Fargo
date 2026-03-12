using Fargo.Application.Common;
using Fargo.Application.Extensions;
using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    /// <summary>
    /// Query used to retrieve all user groups associated with a specific user.
    /// </summary>
    /// <param name="UserGuid">
    /// The unique identifier of the user whose groups should be retrieved.
    /// </param>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the user's groups
    /// as they existed at the specified date and time.
    /// </param>
    public sealed record UserUserGroupsManyQuery(
            Guid UserGuid,
            DateTimeOffset? TemporalAsOf = null
            ) : IQuery<IReadOnlyCollection<UserGroupResponseModel>>;

    /// <summary>
    /// Handles the execution of <see cref="UserUserGroupsManyQuery"/>.
    /// </summary>
    public sealed class UserUserGroupsManyQueryHandler(
            IUserQueries userRepository
            ) : IQueryHandler<UserUserGroupsManyQuery, IReadOnlyCollection<UserGroupResponseModel>>
    {
        /// <summary>
        /// Executes the query to retrieve all user groups associated with a specific user.
        /// </summary>
        /// <param name="query">
        /// The query containing the target user identifier and optional temporal reference.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="UserGroupResponseModel"/> representing
        /// all groups associated with the specified user.
        /// </returns>
        /// <exception cref="UserNotFoundFargoApplicationException">
        /// Thrown when the specified user does not exist.
        /// </exception>
        public async Task<IReadOnlyCollection<UserGroupResponseModel>> Handle(
                UserUserGroupsManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var user = await userRepository.GetByGuid(
                    query.UserGuid,
                    query.TemporalAsOf,
                    cancellationToken
                    )
                ?? throw new UserNotFoundFargoApplicationException(query.UserGuid);

            return [.. user.UserGroups.Select(userGroup => userGroup.ToResponse())];
        }
    }
}