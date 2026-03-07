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
    /// Pagination parameters used to limit and offset the result set.
    /// </param>
    public sealed record UserManyQuery(
            DateTime? TemporalAsOf = null,
            Pagination Pagination = default
            ) : IQuery<IEnumerable<UserResponseModel>>;

    /// <summary>
    /// Handles the execution of <see cref="UserManyQuery"/>.
    /// </summary>
    public sealed class UserManyQueryHandler(
            IUserReadRepository userRepository
            ) : IQueryHandler<UserManyQuery, IEnumerable<UserResponseModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple users.
        /// </summary>
        /// <param name="query">The query containing filtering and pagination parameters.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// A collection of <see cref="UserResponseModel"/> representing the retrieved users.
        /// </returns>
        public async Task<IEnumerable<UserResponseModel>> Handle(
                UserManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var users = await userRepository.GetMany(
                    query.TemporalAsOf,
                    query.Pagination,
                    cancellationToken
                    );

            return [.. users.Select(u => u.ToResponse())];
        }
    }
}