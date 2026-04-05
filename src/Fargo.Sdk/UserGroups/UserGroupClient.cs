using Fargo.Sdk.Http;
using Fargo.Sdk.Models;

namespace Fargo.Sdk.UserGroups;

internal sealed class UserGroupClient : IUserGroupClient
{
    private readonly FargoHttpClient httpClient;

    public UserGroupClient(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<UserGroupInfo?> GetAsync(Guid userGroupGuid, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", asOf?.ToString("o")));

        return httpClient.GetFromJsonAsync<UserGroupInfo>($"/user-groups/{userGroupGuid}{query}", ct);
    }

    public async Task<IReadOnlyCollection<UserGroupInfo>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("userGuid", userGuid?.ToString()),
            ("temporalAsOf", asOf?.ToString("o")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<UserGroupInfo>>($"/user-groups{query}", ct) ?? [];
    }

    public Task<Guid> CreateAsync(
        string nameid,
        string? description = null,
        ActionType[]? permissions = null,
        Guid? firstPartition = null,
        CancellationToken ct = default)
    {
        var permissionsBody = permissions?.Select(a => new { action = (int)a }).ToArray();

        return httpClient.PostFromJsonAsync<object, Guid>(
            "/user-groups",
            new { userGroup = new { nameid, description, permissions = permissionsBody, firstPartition } },
            ct);
    }

    public Task UpdateAsync(
        Guid userGroupGuid,
        string? nameid = null,
        string? description = null,
        bool? isActive = null,
        ActionType[]? permissions = null,
        CancellationToken ct = default)
    {
        var permissionsBody = permissions?.Select(a => new { action = (int)a }).ToArray();

        return httpClient.PatchJsonAsync(
            $"/user-groups/{userGroupGuid}",
            new { nameid, description, isActive, permissions = permissionsBody },
            ct);
    }

    public Task DeleteAsync(Guid userGroupGuid, CancellationToken ct = default)
    {
        return httpClient.DeleteAsync($"/user-groups/{userGroupGuid}", ct);
    }
}
