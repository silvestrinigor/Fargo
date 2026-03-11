using Fargo.Application.Common;
using Fargo.Application.Models.UserGroupModels;

namespace Fargo.Application.Repositories
{
    /// <summary>
    /// Defines query operations for user groups.
    /// </summary>
    public interface IUserGroupQueries
    {
        /// <summary>
        /// Retrieves a single user group by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">
        /// The unique identifier of the user group.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve temporal data.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The matching <see cref="UserGroupReadModel"/>, or <c>null</c> if not found.
        /// </returns>
        Task<UserGroupReadModel?> GetByGuid(
            Guid entityGuid,
            DateTimeOffset? asOfDateTime = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Retrieves a paginated collection of user groups.
        /// </summary>
        /// <param name="pagination">
        /// The pagination parameters used to limit the result set.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve temporal data.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="UserGroupReadModel"/>.
        /// </returns>
        Task<IReadOnlyCollection<UserGroupReadModel>> GetMany(
            Pagination pagination,
            DateTimeOffset? asOfDateTime = null,
            CancellationToken cancellationToken = default
        );
    }
}