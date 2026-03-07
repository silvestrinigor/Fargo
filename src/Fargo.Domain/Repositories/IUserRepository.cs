using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories
{
    /// <summary>
    /// Defines the repository contract for managing <see cref="User"/> entities.
    ///
    /// This repository provides persistence operations and domain queries
    /// related to users.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Gets a user by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">The unique identifier of the user.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<User?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Gets a user by their unique <see cref="Nameid"/>.
        /// </summary>
        /// <param name="nameid">The unique user identifier.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="User"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<User?> GetByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                );

        /// <summary>
        /// Adds a new user to the persistence context.
        /// </summary>
        /// <param name="user">The user to add.</param>
        void Add(User user);

        /// <summary>
        /// Removes a user from the persistence context.
        /// </summary>
        /// <param name="user">The user to remove.</param>
        void Remove(User user);

        /// <summary>
        /// Determines whether any users exist in the system.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// <see langword="true"/> if at least one user exists; otherwise, <see langword="false"/>.
        /// </returns>
        Task<bool> Any(CancellationToken cancellationToken = default);
    }
}