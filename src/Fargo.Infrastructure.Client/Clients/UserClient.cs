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
        CancellationToken cancellationToken = default)
    {
        var uri = $"/users/{userGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<UserInformation?>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<UserInformation>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/users?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";
        return GetCollectionAsync<UserInformation>(uri, cancellationToken);
    }

    public Task<Guid> CreateAsync(
        UserCreateModel model,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<Guid>("/users", model, cancellationToken);
    }

    public Task UpdateAsync(
        Guid userGuid,
        UserUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync($"/users/{userGuid}", model, cancellationToken);
    }

    public Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync($"/users/{userGuid}", cancellationToken);
    }

    public Task AddUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", new { }, cancellationToken);
    }

    public Task RemoveUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", cancellationToken);
    }
}
