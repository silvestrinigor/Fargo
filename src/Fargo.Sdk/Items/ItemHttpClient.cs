using Fargo.Sdk.Contracts.Items;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Items;

/// <summary>Default implementation of <see cref="IItemClient"/>.</summary>
public sealed class ItemHttpClient : IItemClient
{
    /// <summary>Initializes a new instance with the given HTTP client.</summary>
    /// <param name="httpClient">The Fargo HTTP client used to make requests.</param>
    public ItemHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoResponse<ItemInfo>> GetAsync(Guid itemGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<ItemInfo>($"/items/{itemGuid}{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<ItemInfo>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, bool? notChildOfAnyPartition = null, CancellationToken cancellationToken = default)
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

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ItemInfo>>($"/items{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<ItemInfo>());
    }

    /// <inheritdoc />
    public async Task<FargoResponse<Guid>> CreateAsync(ItemCreateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ItemCreateRequest, Guid>(
            "/items",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> UpdateAsync(
        Guid itemGuid,
        ItemUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<ItemUpdateRequest>(
            $"/items/{itemGuid}",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> DeleteAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/items/{itemGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> AddPartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync($"/items/{itemGuid}/partitions/{partitionGuid}", new { }, cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> RemovePartitionAsync(Guid itemGuid, Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/items/{itemGuid}/partitions/{partitionGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetPartitionsAsync(Guid itemGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/items/{itemGuid}/partitions", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<PartitionInfo>());
    }
}
