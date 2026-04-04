using Fargo.Sdk.Http;
using Fargo.Sdk.Models;

namespace Fargo.Sdk.Managers;

internal sealed class UserManager : IUserManager
{
    private readonly FargoHttpClient httpClient;

    internal UserManager(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<UserInfo?> GetAsync(Guid userGuid, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", asOf?.ToString("o")));

        return httpClient.GetFromJsonAsync<UserInfo>($"/users/{userGuid}{query}", ct);
    }

    public async Task<IReadOnlyCollection<UserInfo>> GetManyAsync(DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("temporalAsOf", asOf?.ToString("o")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<UserInfo>>($"/users{query}", ct) ?? [];
    }

    public Task<Guid> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        ActionType[]? permissions = null,
        TimeSpan? defaultPasswordExpiration = null,
        Guid? firstPartition = null,
        CancellationToken ct = default)
    {
        var permissionsBody = permissions?.Select(a => new { action = (int)a }).ToArray();

        return httpClient.PostFromJsonAsync<object, Guid>(
            "/users",
            new
            {
                user = new
                {
                    nameid,
                    password,
                    firstName,
                    lastName,
                    description,
                    permissions = permissionsBody,
                    defaultPasswordExpirationTimeSpan = defaultPasswordExpiration,
                    firstPartition
                }
            },
            ct);
    }

    public Task UpdateAsync(
        Guid userGuid,
        string? nameid = null,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        string? password = null,
        bool? isActive = null,
        ActionType[]? permissions = null,
        TimeSpan? defaultPasswordExpiration = null,
        CancellationToken ct = default)
    {
        var permissionsBody = permissions?.Select(a => new { action = (int)a }).ToArray();

        return httpClient.PatchJsonAsync(
            $"/users/{userGuid}",
            new
            {
                nameid,
                firstName,
                lastName,
                description,
                password,
                isActive,
                permissions = permissionsBody,
                defaultPasswordExpirationPeriod = defaultPasswordExpiration
            },
            ct);
    }

    public Task DeleteAsync(Guid userGuid, CancellationToken ct = default)
    {
        return httpClient.DeleteAsync($"/users/{userGuid}", ct);
    }

    public Task AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken ct = default)
    {
        return httpClient.PostJsonAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", new { }, ct);
    }

    public Task RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken ct = default)
    {
        return httpClient.DeleteAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", ct);
    }
}
