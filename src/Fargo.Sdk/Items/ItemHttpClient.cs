using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemHttpClient"/>.</summary>
public sealed class ItemHttpClient : IItemHttpClient
{
    /// <summary>Initializes a new instance with the given HTTP client.</summary>
    /// <param name="httpClient">The Fargo HTTP client used to make requests.</param>
    public ItemHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ItemInfo>> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<ItemInfo>($"/items/{itemGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ItemInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<ItemInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<ItemInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null, bool? notInsideAnyPartition = null, CancellationToken cancellationToken = default)
    {
        var parameters = new List<(string Key, string? Value)>
        {
            ("temporalAsOfDateTime", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("notInsideAnyPartition", notInsideAnyPartition?.ToString())
        };
        parameters.AddRange(insideAnyOfThisPartitions?.Select(partitionGuid =>
            ("insideAnyOfThisPartitions", (string?)partitionGuid.ToString())) ?? []);
        var query = FargoHttpClient.BuildQuery([.. parameters]);

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ItemInfo>>($"/items{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ItemInfo>>(MapError(httpResponse));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ItemInfo>>(httpResponse.Data ?? []);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(Guid articleGuid, Guid? firstPartition = null, DateTimeOffset? productionDate = null, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ItemCreateRequest, Guid>(
            "/items",
            ContractMappings.ToItemCreateRequest(articleGuid, firstPartition, productionDate),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid> partitions,
        Guid? parentContainerGuid = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<ItemUpdateRequest>(
            $"/items/{itemGuid}",
            ContractMappings.ToItemUpdateRequest(partitions, parentContainerGuid),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/items/{itemGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/items/{itemGuid}/partitions/{partitionGuid}", new { }, cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/items/{itemGuid}/partitions/{partitionGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/items/{itemGuid}/partitions", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>(MapError(httpResponse));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionInfo>>(httpResponse.Data ?? []);
    }

    private static FargoSdkError MapError<T>(FargoSdkHttpResponse<T> response) => FargoSdkProblemMapper.Map(response.Problem, response.StatusCode);
}
