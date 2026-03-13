using Fargo.Application.Common;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Models.UserModels;
using Fargo.HttpApi.Client.Interfaces;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class UserClient(HttpClient http)
    : FargoHttpClientBase(http), IUserClient
{
    public Task<UserResponseModel?> GetSingleAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
        => GetAsync<UserResponseModel>($"/users/{userGuid}?temporalAsOf={temporalAsOf}", ct);

    public Task<IReadOnlyCollection<UserResponseModel>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri = $"/users?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<UserResponseModel>(uri, ct);
    }

    public Task<IReadOnlyCollection<UserGroupResponseModel>> GetUserGroupsAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
    {
        var uri = $"/users/{userGuid}/user-groups?temporalAsOf={temporalAsOf}";

        return GetCollectionAsync<UserGroupResponseModel>(uri, ct);
    }

    public Task<Guid> CreateAsync(UserCreateModel model, CancellationToken ct = default)
        => PostAsync<Guid>("/users", model, ct);

    public Task UpdateAsync(Guid userGuid, UserUpdateModel model, CancellationToken ct = default)
        => PatchAsync($"/users/{userGuid}", model, ct);

    public Task DeleteAsync(Guid userGuid, CancellationToken ct = default)
        => DeleteAsync($"/users/{userGuid}", ct);

    public Task AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken ct = default)
        => PostAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", new { }, ct);

    public Task RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken ct = default)
        => DeleteAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", ct);
}