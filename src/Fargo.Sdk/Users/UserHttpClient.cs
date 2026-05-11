using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Users;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Users;

/// <summary>Default implementation of <see cref="IUserClient"/>.</summary>
public sealed class UserHttpClient : IUserClient
{
    /// <summary>Initializes a new instance.</summary>
    public UserHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoResponse<UserInfo>> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<UserInfo>($"/users/{userGuid}{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<UserInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, bool? notChildOfAnyPartition = null, CancellationToken cancellationToken = default)
    {
        var parameters = new List<(string Key, string? Value)>
        {
            ("temporalAsOfDateTime", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("notChildOfAnyPartition", notChildOfAnyPartition?.ToString())
        };
        parameters.AddRange(childOfAnyOfThesePartitions?.Select(partitionGuid =>
            ("childOfAnyOfThesePartitions", (string?)partitionGuid.ToString())) ?? []);
        var query = FargoHttpClient.BuildQuery([.. parameters]);

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<UserInfo>>($"/users{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<UserInfo>());
    }

    /// <inheritdoc />
    public async Task<FargoResponse<Guid>> CreateAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<UserCreateRequest, Guid>(
            "/users",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> UpdateAsync(Guid userGuid, UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<UserUpdateRequest>(
            $"/users/{userGuid}",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/users/{userGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", new { }, cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/users/{userGuid}/user-groups/{userGroupGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> AddPartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/users/{userGuid}/partitions/{partitionGuid}", new { }, cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> RemovePartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/users/{userGuid}/partitions/{partitionGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid userGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/users/{userGuid}/partitions", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<PartitionInfo>());
    }
}
