using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.UserGroups;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupHttpClient"/>.</summary>
public sealed class UserGroupHttpClient : IUserGroupHttpClient
{
    /// <summary>Initializes a new instance.</summary>
    public UserGroupHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<UserGroupInfo>> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<UserGroupInfo>($"/user-groups/{userGroupGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<UserGroupInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<UserGroupInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<UserGroupInfo>>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("temporalAsOfDateTime", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<UserGroupInfo>>($"/user-groups{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<UserGroupInfo>>(MapError(httpResponse));
        }

        return new FargoSdkResponse<IReadOnlyCollection<UserGroupInfo>>(httpResponse.Data ?? []);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<UserGroupCreateRequest, Guid>(
            "/user-groups",
            ContractMappings.ToUserGroupCreateRequest(nameid, description, permissions, firstPartition),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGroupGuid, string? nameid = null, string? description = null, bool? isActive = null, IReadOnlyCollection<ActionType>? permissions = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<UserGroupUpdateRequest>(
            $"/user-groups/{userGroupGuid}",
            ContractMappings.ToUserGroupUpdateRequest(nameid, description, isActive, permissions),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/user-groups/{userGroupGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/user-groups/{userGroupGuid}/partitions", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>(MapError(httpResponse));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>(httpResponse.Data ?? []);
    }

    private static FargoSdkError MapError<T>(FargoSdkHttpResponse<T> response) => FargoSdkProblemMapper.Map(response.Problem, response.StatusCode);
}
