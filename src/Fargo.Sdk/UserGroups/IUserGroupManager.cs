namespace Fargo.Sdk.UserGroups;

/// <summary>
/// Provides high-level operations for managing user groups.
/// Returns live <see cref="UserGroup"/> entities whose property setters automatically
/// persist changes to the backend.
/// </summary>
public interface IUserGroupManager
{
    /// <summary>Gets a single user group by its unique identifier.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the user group does not exist or is not accessible.</exception>
    Task<UserGroup> GetAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a paginated list of user groups accessible to the current user.</summary>
    /// <param name="userGuid">When provided, returns only groups the specified user belongs to.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<UserGroup>> GetManyAsync(
        Guid? userGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new user group and returns it as a live entity.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<UserGroup> CreateAsync(
        string nameid,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes a user group.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the user group cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Raised when any authenticated client creates a user group.</summary>
    event EventHandler<UserGroupCreatedEventArgs>? Created;
}
