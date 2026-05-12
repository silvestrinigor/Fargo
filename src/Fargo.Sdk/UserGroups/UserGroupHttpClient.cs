using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.UserGroups;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupClient"/>.</summary>
public sealed class UserGroupHttpClient : IUserGroupClient
{
    /// <summary>Initializes a new instance.</summary>
    public UserGroupHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoResponse<UserGroupInfo>> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<UserGroupInfo>($"/user-groups/{userGroupGuid}{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<UserGroupInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, bool? notChildOfAnyPartition = null, CancellationToken cancellationToken = default)
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

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<UserGroupInfo>>($"/user-groups{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<UserGroupInfo>());
    }

    /// <inheritdoc />
    public async Task<FargoResponse<Guid>> CreateAsync(UserGroupCreateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<UserGroupCreateRequest, Guid>(
            "/user-groups",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> UpdateAsync(Guid userGroupGuid, UserGroupUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<UserGroupUpdateRequest>(
            $"/user-groups/{userGroupGuid}",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/user-groups/{userGroupGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/user-groups/{userGroupGuid}/partitions", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<PartitionInfo>());
    }
}
