namespace Fargo.Sdk.Users;

/// <summary>
/// Provides high-level operations for managing users.
/// Returns live <see cref="User"/> entities whose property setters automatically
/// persist changes to the backend.
/// </summary>
public interface IUserManager
{
    /// <summary>Gets a single user by their unique identifier.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the user does not exist or is not accessible.</exception>
    Task<User> GetAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paginated list of users accessible to the current user.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<User>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new user and returns it as a live entity.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<User> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a user.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the user cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Raised when any authenticated client creates a user.</summary>
    event EventHandler<UserCreatedEventArgs>? Created;

    /// <summary>Raised when any authenticated client updates a user.</summary>
    event EventHandler<UserUpdatedEventArgs>? Updated;

    /// <summary>Raised when any authenticated client deletes a user.</summary>
    event EventHandler<UserDeletedEventArgs>? Deleted;
}
