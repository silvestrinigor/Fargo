using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class UserClient(HttpClient http)
    : FargoHttpClientBase(http), IUserClient
{
    public Task<UserInformation?> GetSingleAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
        => GetAsync<UserInformation>($"/users/{userGuid}?temporalAsOf={temporalAsOf}", ct);

    public Task<IReadOnlyCollection<UserInformation>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri = $"/users?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<UserInformation>(uri, ct);
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