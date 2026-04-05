using Fargo.Sdk.Models;

namespace Fargo.Sdk.UserGroups;

public interface IUserGroupManager
{
    Task<UserGroupInfo?> GetAsync(Guid userGroupGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<UserGroupInfo>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateAsync(
        string nameid,
        string? description = null,
        ActionType[]? permissions = null,
        Guid? firstPartition = null,
        CancellationToken ct = default);

    Task UpdateAsync(
        Guid userGroupGuid,
        string? nameid = null,
        string? description = null,
        bool? isActive = null,
        ActionType[]? permissions = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid userGroupGuid, CancellationToken ct = default);
}
