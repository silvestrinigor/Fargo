using Fargo.Application.Common;
using Fargo.Application.Models.UserGroupModels;

namespace Fargo.HttpApi.Client.Interfaces
{
    /// <summary>
    /// Defines the contract for user-group-related HTTP API operations.
    /// </summary>
    public interface IUserGroupClient
    {
        /// <summary>
        /// Gets a single user group by its identifier.
        /// </summary>
        Task<UserGroupResponseModel?> GetSingleAsync(
            Guid userGroupGuid,
            DateTimeOffset? temporalAsOf = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets multiple user groups with optional pagination and temporal query.
        /// </summary>
        Task<IReadOnlyCollection<UserGroupResponseModel>> GetManyAsync(
            DateTimeOffset? temporalAsOf = null,
            Page? page = null,
            Limit? limit = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user group and returns its identifier.
        /// </summary>
        Task<Guid> CreateAsync(
            UserGroupCreateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing user group.
        /// </summary>
        Task UpdateAsync(
            Guid userGroupGuid,
            UserGroupUpdateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user group.
        /// </summary>
        Task DeleteAsync(
            Guid userGroupGuid,
            CancellationToken cancellationToken = default);
    }
}