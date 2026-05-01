using Fargo.Api.Contracts.Partitions;
using Fargo.Api.Contracts.UserGroups;
using Fargo.Api.Http;
using Fargo.Api.Partitions;
using Fargo.Sdk;

namespace Fargo.Api.UserGroups;

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
    public async Task<FargoSdkResponse<UserGroupResult>> GetAsync(Guid userGroupGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<UserGroupDto>($"/user-groups/{userGroupGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<UserGroupResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<UserGroupResult>(httpResponse.Data!.ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>> GetManyAsync(Guid? userGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("userGuid", userGuid?.ToString()),
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<UserGroupDto>>($"/user-groups{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>((httpResponse.Data ?? []).ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(string nameid, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, Guid? firstPartition = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<UserGroupCreateDto, Guid>(
            "/user-groups",
            ContractMappings.ToUserGroupCreateDto(nameid, description, permissions, firstPartition),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGroupGuid, string? nameid = null, string? description = null, bool? isActive = null, IReadOnlyCollection<ActionType>? permissions = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync<UserGroupUpdateDto>(
            $"/user-groups/{userGroupGuid}",
            ContractMappings.ToUserGroupUpdateDto(nameid, description, isActive, permissions),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/user-groups/{userGroupGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionDto>>($"/user-groups/{userGroupGuid}/partitions", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>((httpResponse.Data ?? []).ToSdk());
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
