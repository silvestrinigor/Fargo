using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;

namespace Fargo.Application.Repositories
{
    /// <summary>
    /// Provides read operations for <see cref="UserReadModel"/>.
    /// </summary>
    /// <remarks>
    /// This repository belongs to the query side of the application (CQRS)
    /// and is responsible only for retrieving user data.
    /// It must not modify the state of the system.
    /// </remarks>
    public interface IUserReadRepository
    {
        /// <summary>
        /// Retrieves a single user by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the user.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the user
        /// as it existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="UserReadModel"/> if found; otherwise <c>null</c>.
        /// </returns>
        Task<UserReadModel?> GetByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Retrieves multiple users using pagination.
        /// </summary>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the result represents the state of the users
        /// as they existed at the specified date and time.
        /// </param>
        /// <param name="pagination">
        /// Pagination parameters used to limit and offset the result set.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="UserReadModel"/>.
        /// </returns>
        Task<IReadOnlyCollection<UserReadModel>> GetMany(
                DateTimeOffset? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                );
    }
}