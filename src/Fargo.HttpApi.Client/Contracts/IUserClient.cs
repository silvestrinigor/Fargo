using Fargo.Application.Common;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Models.UserModels;

namespace Fargo.HttpApi.Client.Interfaces
{
    /// <summary>
    /// Defines the contract for user-related HTTP API operations.
    /// </summary>
    public interface IUserClient
    {
        /// <summary>
        /// Gets a single user by its identifier.
        /// </summary>
        Task<UserResponseModel?> GetSingleAsync(
            Guid userGuid,
            DateTimeOffset? temporalAsOf = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets multiple users with optional pagination and temporal query.
        /// </summary>
        Task<IReadOnlyCollection<UserResponseModel>> GetManyAsync(
            DateTimeOffset? temporalAsOf = null,
            Page? page = null,
            Limit? limit = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all user groups associated with a user.
        /// </summary>
        Task<IReadOnlyCollection<UserGroupResponseModel>> GetUserGroupsAsync(
            Guid userGuid,
            DateTimeOffset? temporalAsOf = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user and returns its identifier.
        /// </summary>
        Task<Guid> CreateAsync(
            UserCreateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing user.
        /// </summary>
        Task UpdateAsync(
            Guid userGuid,
            UserUpdateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user.
        /// </summary>
        Task DeleteAsync(
            Guid userGuid,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user group to a user.
        /// </summary>
        Task AddUserGroupAsync(
            Guid userGuid,
            Guid userGroupGuid,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user group from a user.
        /// </summary>
        Task RemoveUserGroupAsync(
            Guid userGuid,
            Guid userGroupGuid,
            CancellationToken cancellationToken = default);
    }
}