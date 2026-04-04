using Fargo.Sdk.Models;

namespace Fargo.Sdk.Managers;

public interface IUserManager
{
    Task<UserInfo?> GetAsync(Guid userGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<UserInfo>> GetManyAsync(DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        ActionType[]? permissions = null,
        TimeSpan? defaultPasswordExpiration = null,
        Guid? firstPartition = null,
        CancellationToken ct = default);

    Task UpdateAsync(
        Guid userGuid,
        string? nameid = null,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        string? password = null,
        bool? isActive = null,
        ActionType[]? permissions = null,
        TimeSpan? defaultPasswordExpiration = null,
        CancellationToken ct = default);

    Task DeleteAsync(Guid userGuid, CancellationToken ct = default);

    Task AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken ct = default);

    Task RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken ct = default);
}
