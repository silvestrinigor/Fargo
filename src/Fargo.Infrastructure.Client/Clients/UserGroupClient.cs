using Fargo.Application.Models.UserGroupModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class UserGroupClient(HttpClient http)
    : FargoHttpClientBase(http), IUserGroupClient
{
    public Task<UserGroupInformation?> GetSingleAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/user-groups/{userGroupGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<UserGroupInformation?>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<UserGroupInformation>> GetManyAsync(
        Guid? userGuid,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri =
            $"/user-groups?userGuid={userGuid}&temporalAsOf={temporalAsOf}&page={page}&limit={limit}";

        return GetCollectionAsync<UserGroupInformation>(uri, cancellationToken);
    }

    public Task<Guid> CreateAsync(
        UserGroupCreateModel model,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<Guid>("/user-groups", model, cancellationToken);
    }

    public Task UpdateAsync(
        Guid userGroupGuid,
        UserGroupUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync($"/user-groups/{userGroupGuid}", model, cancellationToken);
    }

    public Task DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync($"/user-groups/{userGroupGuid}", cancellationToken);
    }
}
