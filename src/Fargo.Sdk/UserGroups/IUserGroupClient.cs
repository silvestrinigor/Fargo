using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.UserGroups;

/// <summary>
/// Provides operations for managing user groups.
/// </summary>
/// <remarks>
/// User groups allow permissions to be assigned collectively to multiple users.
/// A user can belong to multiple groups and inherits all permissions from each group.
/// </remarks>
public interface IUserGroupClient
{
    /// <summary>
    /// Gets a single user group by its unique identifier.
    /// </summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="temporalAsOf">
    /// When provided, returns the state of the user group as it existed at this point in time.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the <see cref="UserGroupResult"/>, or a <see cref="FargoSdkErrorType.NotFound"/>
    /// error if the user group does not exist or is not accessible.
    /// </returns>
    Task<FargoSdkResponse<UserGroupResult>> GetAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of user groups accessible to the current user.
    /// </summary>
    /// <param name="userGuid">When provided, returns only groups that the specified user belongs to.</param>
    /// <param name="temporalAsOf">When provided, returns historical data as of this point in time.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the collection of user groups. The collection is empty if none are found.</returns>
    Task<FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>> GetManyAsync(
        Guid? userGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user group.
    /// </summary>
    /// <param name="nameid">The unique name identifier of the group.</param>
    /// <param name="description">An optional description of the group.</param>
    /// <param name="permissions">
    /// The set of permissions to assign to this group, or <see langword="null"/> to assign none.
    /// </param>
    /// <param name="firstPartition">
    /// The partition to associate with the group on creation.
    /// When <see langword="null"/>, the global partition is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the <see cref="Guid"/> of the newly created user group.</returns>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string nameid,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user group. Only the properties provided will be changed.
    /// </summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to update.</param>
    /// <param name="nameid">The new name identifier, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">The new description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="isActive">Whether the group should be active, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="permissions">
    /// The new set of permissions, or <see langword="null"/> to leave unchanged.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid userGroupGuid,
        string? nameid = null,
        string? description = null,
        bool? isActive = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user group.
    /// </summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the partitions that directly contain the specified user group.
    /// </summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the collection of <see cref="PartitionResult"/> values, or a
    /// <see cref="FargoSdkErrorType.NotFound"/> error if the user group does not exist.
    /// The collection is empty if the user group exists but the current user has no partition access overlap.
    /// </returns>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);
}
