using Fargo.Application.Models.UserGroupModels;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class UserGroupClient(HttpClient http)
    : FargoHttpClientBase(http), IUserGroupClient
{
    public Task<UserGroupInformation?> GetSingleAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
        => GetAsync<UserGroupInformation>(
            $"/user-groups/{userGroupGuid}?temporalAsOf={temporalAsOf}", ct);

    public Task<IReadOnlyCollection<UserGroupInformation>> GetManyAsync(
        Guid? userGuid,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri = $"/user-groups?userGuid={userGuid}&temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<UserGroupInformation>(uri, ct);
    }

    public Task<Guid> CreateAsync(UserGroupCreateModel model, CancellationToken ct = default)
        => PostAsync<Guid>("/user-groups", model, ct);

    public Task UpdateAsync(Guid userGroupGuid, UserGroupUpdateModel model, CancellationToken ct = default)
        => PatchAsync($"/user-groups/{userGroupGuid}", model, ct);

    public Task DeleteAsync(Guid userGroupGuid, CancellationToken ct = default)
        => DeleteAsync($"/user-groups/{userGroupGuid}", ct);
}