using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="RefreshToken"/> entities.
///
/// This repository provides access to refresh token persistence operations
/// and queries used during authentication flows.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the refresh token.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="RefreshToken"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<RefreshToken?> GetByGuid(
            Guid entityGuid,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Gets a refresh token by its hashed token value.
    /// </summary>
    /// <param name="tokenHash">The hashed token used to identify the refresh token.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="RefreshToken"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<RefreshToken?> GetByTokenHash(
            TokenHash tokenHash,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Adds a new refresh token to the persistence context.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    void Add(RefreshToken refreshToken);

    /// <summary>
    /// Removes a refresh token from the persistence context.
    /// </summary>
    /// <param name="refreshToken">The refresh token to remove.</param>
    void Remove(RefreshToken refreshToken);
}
