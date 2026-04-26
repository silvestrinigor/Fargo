using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Users;

/// <summary>Low-level HTTP transport for user endpoints.</summary>
public interface IUserHttpClient
{
    /// <summary>Retrieves a single user by their unique identifier.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<UserResult>> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of users.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="partitionGuid">Filter to users in this partition.</param>
    /// <param name="search">An optional search term to filter by name identifier.</param>
    /// <param name="noPartition">When <see langword="true"/>, returns only users without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<UserResult>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new user and returns the assigned identifier.</summary>
    /// <param name="nameid">The login name identifier for the user.</param>
    /// <param name="password">The initial password.</param>
    /// <param name="firstName">Optional first name.</param>
    /// <param name="lastName">Optional last name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="permissions">Optional initial set of action permissions.</param>
    /// <param name="defaultPasswordExpirationPeriod">Optional default period before password expiry.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing user.</summary>
    /// <param name="userGuid">The unique identifier of the user to update.</param>
    /// <param name="nameid">New name identifier, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="firstName">New first name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="lastName">New last name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">New description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="password">New password, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="isActive">New active state, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="permissions">New permission set, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="defaultPasswordExpirationPeriod">New expiration period, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGuid, string? nameid = null, string? firstName = null, string? lastName = null, string? description = null, string? password = null, bool? isActive = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user by their unique identifier.</summary>
    /// <param name="userGuid">The unique identifier of the user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a user group membership to a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>Removes a user group membership from a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a partition access to a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Removes a partition access from a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions accessible to a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid userGuid, CancellationToken cancellationToken = default);
}
