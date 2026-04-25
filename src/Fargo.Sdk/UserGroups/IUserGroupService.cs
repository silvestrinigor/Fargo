namespace Fargo.Sdk.UserGroups;

/// <summary>Provides CRUD operations for user groups and routes hub Updated/Deleted events to tracked entities.</summary>
public interface IUserGroupService
{
    Task<UserGroup> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserGroup>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, CancellationToken cancellationToken = default);

    Task<UserGroup> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);
}
