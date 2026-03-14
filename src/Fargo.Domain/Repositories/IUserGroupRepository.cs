using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Repositories
{
    /// <summary>
    /// Defines the repository contract for managing <see cref="UserGroup"/> entities.
    ///
    /// This repository provides persistence operations and domain queries
    /// related to user groups.
    /// </summary>
    public interface IUserGroupRepository
    {
        /// <summary>
        /// Gets a user group by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">The unique identifier of the user group.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="UserGroup"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<UserGroup?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Gets a user group by its unique <see cref="Nameid"/>.
        /// </summary>
        /// <param name="nameid">The unique user group identifier.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="UserGroup"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<UserGroup?> GetByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Determines whether a user group with the specified <see cref="Nameid"/> already exists.
        /// </summary>
        /// <param name="nameid">
        /// The unique user group identifier to check.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a user group with the specified <see cref="Nameid"/> exists;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> ExistsByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                );

        Task<UserGroupInformation?> GetInfoByGuid(
                Guid entityGuid,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );

        Task<IReadOnlyCollection<UserGroupInformation>> GetManyInfo(
                Pagination pagination,
                Guid? userGuid = null,
                DateTimeOffset? asOfDateTime = null,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Adds a new user group to the persistence context.
        /// </summary>
        /// <param name="userGroup">The user group to add.</param>
        void Add(UserGroup userGroup);

        /// <summary>
        /// Removes a user group from the persistence context.
        /// </summary>
        /// <param name="userGroup">The user group to remove.</param>
        void Remove(UserGroup userGroup);

        /// <summary>
        /// Determines whether any user groups exist in the system.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if at least one user group exists; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> Any(CancellationToken cancellationToken = default);
    }
}