using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Users;

namespace Fargo.Sdk.Users;

/// <summary>Low-level HTTP transport for user endpoints.</summary>
public interface IUserClient
{
    /// <summary>Retrieves a single user by their unique identifier.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<UserInfo>> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of users.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="insideAnyOfThisPartitions">Filters to users inside any of these partitions.</param>
    /// <param name="notInsideAnyPartition">When <see langword="true"/>, includes users without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<IReadOnlyCollection<UserInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null, bool? notInsideAnyPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new user and returns the assigned identifier.</summary>
    /// <param name="request">The user creation request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<Guid>> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing user.</summary>
    /// <param name="userGuid">The unique identifier of the user to update.</param>
    /// <param name="request">The user update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> UpdateAsync(Guid userGuid, UserUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user by their unique identifier.</summary>
    /// <param name="userGuid">The unique identifier of the user to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a user group membership to a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>Removes a user group membership from a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="userGroupGuid">The unique identifier of the user group to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a partition access to a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> AddPartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Removes a partition access from a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="partitionGuid">The unique identifier of the partition to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse> RemovePartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    /// <summary>Returns the partitions accessible to a user.</summary>
    /// <param name="userGuid">The unique identifier of the user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid userGuid, CancellationToken cancellationToken = default);
}
