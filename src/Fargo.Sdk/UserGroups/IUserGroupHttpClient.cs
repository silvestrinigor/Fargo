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
    /// <param name="request">The user group creation request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(UserGroupCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing user group.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to update.</param>
    /// <param name="request">The user group update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGroupGuid, UserGroupUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user group by its unique identifier.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions accessible to a user group.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);
}
