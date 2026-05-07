using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionHttpClient"/>.</summary>
public sealed class PartitionHttpClient : IPartitionHttpClient
{
    /// <summary>Initializes a new instance.</summary>
    public PartitionHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<PartitionInfo>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<PartitionInfo>($"/partitions/{partitionGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<PartitionInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<PartitionInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("temporalAsOfDateTime", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/partitions{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>(MapError(httpResponse));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>(httpResponse.Data ?? []);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<PartitionCreateRequest, Guid>(
            "/partitions",
            ContractMappings.ToPartitionCreateRequest(name, description, parentPartitionGuid),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid partitionGuid, string? name = null, string? description = null, Guid? parentPartitionGuid = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<PartitionUpdateRequest>(
            $"/partitions/{partitionGuid}",
            ContractMappings.ToPartitionUpdateRequest(name, description, parentPartitionGuid, isActive),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/partitions/{partitionGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError<T>(FargoSdkHttpResponse<T> response) => FargoSdkProblemMapper.Map(response.Problem, response.StatusCode);
}
