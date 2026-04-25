namespace Fargo.Sdk.UserGroups;

/// <summary>Provides CRUD operations for user groups and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IUserGroupService
{
    /// <summary>Retrieves a single user group by its unique identifier.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the user group does not exist or is not accessible.</exception>
    Task<UserGroup> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged list of user groups, optionally filtered by user membership.</summary>
    /// <param name="userGuid">Filter to groups that contain this user.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<UserGroup>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new user group and returns it as a live entity.</summary>
    /// <param name="nameid">The name identifier for the group.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="permissions">Optional initial set of action permissions.</param>
    /// <param name="firstPartition">An optional initial partition to assign.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<UserGroup> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a user group by its unique identifier.</summary>
    /// <param name="userGroupGuid">The unique identifier of the user group to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the group cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);
}
