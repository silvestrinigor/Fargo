using Fargo.Application.Common;
using Fargo.Application.Extensions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserGroupQueries
{
    /// <summary>
    /// Query used to retrieve multiple user groups.
    /// </summary>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the user groups
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="Pagination">
    /// Optional pagination parameters used to limit and offset the result set.
    /// If not provided, a default pagination configuration is used.
    /// </param>
    public sealed record UserGroupManyQuery(
            DateTimeOffset? TemporalAsOf = null,
            Pagination? Pagination = null
            ) : IQuery<IReadOnlyCollection<UserGroupResponseModel>>;

    /// <summary>
    /// Handles the execution of <see cref="UserGroupManyQuery"/>.
    /// </summary>
    public sealed class UserGroupManyQueryHandler(
            IUserGroupQueries userGroupRepository
            ) : IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupResponseModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple user groups.
        /// </summary>
        /// <param name="query">
        /// The query containing the temporal reference and optional pagination parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="UserGroupResponseModel"/> representing
        /// the user groups that match the specified temporal reference and pagination.
        /// </returns>
        /// <remarks>
        /// If pagination is not provided, the query uses
        /// <see cref="Pagination.First20Pages"/> as the default.
        /// </remarks>
        public async Task<IReadOnlyCollection<UserGroupResponseModel>> Handle(
                UserGroupManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var userGroups = await userGroupRepository.GetMany(
                    query.Pagination ?? Pagination.First20Pages,
                    query.TemporalAsOf,
                    cancellationToken
                    );

            return [.. userGroups.Select(userGroup => userGroup.ToResponse())];
        }
    }
}