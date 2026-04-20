using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Users;

/// <summary>
/// Provides operations for managing users.
/// </summary>
public interface IUserClient
{
    /// <summary>
    /// Gets a single user by their unique identifier.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="temporalAsOf">
    /// When provided, returns the state of the user as it existed at this point in time.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the <see cref="UserResult"/>, or a <see cref="FargoSdkErrorType.NotFound"/>
    /// error if the user does not exist or is not accessible.
    /// </returns>
    Task<FargoSdkResponse<UserResult>> GetAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of users accessible to the current user.
    /// </summary>
    /// <param name="temporalAsOf">When provided, returns historical data as of this point in time.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the collection of users. The collection is empty if none are found.</returns>
    Task<FargoSdkResponse<IReadOnlyCollection<UserResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="nameid">The unique name identifier used to log in.</param>
    /// <param name="password">The initial password for the user.</param>
    /// <param name="firstName">The user's first name, or <see langword="null"/> to leave unset.</param>
    /// <param name="lastName">The user's last name, or <see langword="null"/> to leave unset.</param>
    /// <param name="description">An optional description of the user.</param>
    /// <param name="permissions">
    /// The set of permissions to assign directly to this user, or <see langword="null"/> to assign none.
    /// </param>
    /// <param name="defaultPasswordExpirationPeriod">
    /// How long the user's password is valid before requiring a change.
    /// When <see langword="null"/>, the server default is used.
    /// </param>
    /// <param name="firstPartition">
    /// The partition to associate with the user on creation.
    /// When <see langword="null"/>, the global partition is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the <see cref="Guid"/> of the newly created user.</returns>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user. Only the properties provided will be changed.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user to update.</param>
    /// <param name="nameid">The new login name identifier, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="firstName">The new first name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="lastName">The new last name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">The new description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="password">The new password, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="isActive">Whether the account should be active, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="permissions">
    /// The new set of direct permissions, or <see langword="null"/> to leave unchanged.
    /// </param>
    /// <param name="defaultPasswordExpirationPeriod">
    /// The new password expiration period, or <see langword="null"/> to leave unchanged.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid userGuid,
        string? nameid = null,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        string? password = null,
        bool? isActive = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds the specified user to a user group.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group to add the user to.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the specified user from a user group.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group to remove the user from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a partition to the specified user.</summary>
    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid userGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a partition from the specified user.</summary>
    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid userGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the partitions that directly contain the specified user.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the collection of <see cref="PartitionResult"/> values, or a
    /// <see cref="FargoSdkErrorType.NotFound"/> error if the user does not exist.
    /// The collection is empty if the user exists but the current user has no partition access overlap.
    /// </returns>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default);
}
