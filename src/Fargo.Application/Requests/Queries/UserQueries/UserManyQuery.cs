using Fargo.Application.Commom;
using Fargo.Application.Extensions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    /// <summary>
    /// Query used to retrieve multiple users.
    /// </summary>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the users
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="Pagination">
    /// Optional pagination parameters used to limit and offset the result set.
    /// If not provided, a default pagination configuration is used.
    /// </param>
    public sealed record UserManyQuery(
            DateTimeOffset? TemporalAsOf = null,
            Pagination? Pagination = null
            ) : IQuery<IReadOnlyCollection<UserResponseModel>>;

    /// <summary>
    /// Handles the execution of <see cref="UserManyQuery"/>.
    /// </summary>
    public sealed class UserManyQueryHandler(
            IUserReadRepository userRepository
            ) : IQueryHandler<UserManyQuery, IReadOnlyCollection<UserResponseModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple users.
        /// </summary>
        /// <param name="query">
        /// The query containing the temporal reference and optional pagination parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="UserResponseModel"/> representing
        /// the users that match the specified temporal reference and pagination.
        /// </returns>
        /// <remarks>
        /// If pagination is not provided, the query uses
        /// <see cref="Pagination.First20Pages"/> as the default.
        /// </remarks>
        public async Task<IReadOnlyCollection<UserResponseModel>> Handle(
                UserManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var users = await userRepository.GetMany(
                    query.Pagination ?? Pagination.First20Pages,
                    query.TemporalAsOf,
                    cancellationToken
                    );

            return [.. users.Select(u => u.ToResponse())];
        }
    }
}