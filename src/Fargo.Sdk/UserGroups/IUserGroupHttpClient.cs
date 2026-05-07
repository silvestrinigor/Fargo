using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.UserGroups;

namespace Fargo.Sdk.UserGroups;

/// <summary>Low-level HTTP transport for user group endpoints.</summary>
public interface IUserGroupHttpClient
{
    /// <summary>Retrieves a single user group by its unique identifier.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<UserGroupInfo>> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of user groups.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="insideAnyOfThisPartitions">Filters to user groups inside any of these partitions.</param>
    /// <param name="notInsideAnyPartition">When <see langword="true"/>, includes user groups without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<UserGroupInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null, bool? notInsideAnyPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new user group and returns its assigned identifier.</summary>
    /// <param name="nameid">The name identifier for the group.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="permissions">Optional initial set of action permissions.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing user group.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to update.</param>
    /// <param name="nameid">New name identifier, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">New description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="isActive">New active state, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="permissions">New permission set, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGroupGuid, string? nameid = null, string? description = null, bool? isActive = null, IReadOnlyCollection<ActionType>? permissions = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user group by its unique identifier.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions accessible to a user group.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);
}
