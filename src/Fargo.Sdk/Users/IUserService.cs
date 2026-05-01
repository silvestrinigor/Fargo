namespace Fargo.Api.Users;

/// <summary>Provides CRUD operations for users and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IUserService
{
    /// <summary>Retrieves a single user by their unique identifier.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the user does not exist or is not accessible.</exception>
    Task<User> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of users.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="partitionGuid">Filter to users in this partition.</param>
    /// <param name="search">An optional search term to filter by name identifier.</param>
    /// <param name="noPartition">When <see langword="true"/>, returns only users without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<User>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new user and returns them as a live entity.</summary>
    /// <param name="nameid">The login name identifier.</param>
    /// <param name="password">The initial password.</param>
    /// <param name="firstName">Optional first name.</param>
    /// <param name="lastName">Optional last name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="permissions">Optional initial set of action permissions.</param>
    /// <param name="defaultPasswordExpirationPeriod">Optional default period before password expiry.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<User> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user by their unique identifier.</summary>
    /// <param name="userGuid">The unique identifier of the user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the user cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);
}
