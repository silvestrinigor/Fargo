using Fargo.Application.Extensions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    /// <summary>
    /// Query used to retrieve a single user by its unique identifier.
    /// </summary>
    /// <param name="UserGuid">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the user
    /// as it existed at the specified date and time.
    /// </param>
    public sealed record UserSingleQuery(
            Guid UserGuid,
            DateTime? TemporalAsOf = null
            ) : IQuery<UserResponseModel?>;

    /// <summary>
    /// Handles the execution of <see cref="UserSingleQuery"/>.
    /// </summary>
    public sealed class UserSingleQueryHandler(
            IUserReadRepository userRepository
            ) : IQueryHandler<UserSingleQuery, UserResponseModel?>
    {
        /// <summary>
        /// Executes the query to retrieve a single user.
        /// </summary>
        /// <param name="query">The query containing the user identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The <see cref="UserResponseModel"/> if the user exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<UserResponseModel?> Handle(
                UserSingleQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var user = await userRepository.GetByGuid(
                    query.UserGuid,
                    query.TemporalAsOf,
                    cancellationToken
                    );

            return user?.ToResponse();
        }
    }
}