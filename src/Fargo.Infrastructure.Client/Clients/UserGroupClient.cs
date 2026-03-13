using Fargo.Application.Common;
using Fargo.Application.Models.UserGroupModels;
using Fargo.HttpApi.Client.Interfaces;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class UserGroupClient(HttpClient http)
    : FargoHttpClientBase(http), IUserGroupClient
{
    public Task<UserGroupResponseModel?> GetSingleAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
        => GetAsync<UserGroupResponseModel>(
            $"/user-groups/{userGroupGuid}?temporalAsOf={temporalAsOf}", ct);

    public Task<IReadOnlyCollection<UserGroupResponseModel>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri = $"/user-groups?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<UserGroupResponseModel>(uri, ct);
    }

    public Task<Guid> CreateAsync(UserGroupCreateModel model, CancellationToken ct = default)
        => PostAsync<Guid>("/user-groups", model, ct);

    public Task UpdateAsync(Guid userGroupGuid, UserGroupUpdateModel model, CancellationToken ct = default)
        => PatchAsync($"/user-groups/{userGroupGuid}", model, ct);

    public Task DeleteAsync(Guid userGroupGuid, CancellationToken ct = default)
        => DeleteAsync($"/user-groups/{userGroupGuid}", ct);
}