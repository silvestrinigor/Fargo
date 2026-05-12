using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionClient"/>.</summary>
public sealed class PartitionHttpClient : IPartitionClient
{
    /// <summary>Initializes a new instance.</summary>
    public PartitionHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoResponse<PartitionInfo>> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<PartitionInfo>($"/partitions/{partitionGuid}{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<PartitionInfo>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
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
        var query = FargoHttpClient.BuildQuery(
            [
                .. parameters
            ]);

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionInfo>>($"/partitions{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<PartitionInfo>());
    }

    /// <inheritdoc />
    public async Task<FargoResponse<Guid>> CreateAsync(PartitionCreateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<PartitionCreateRequest, Guid>(
            "/partitions",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> UpdateAsync(Guid partitionGuid, PartitionUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<PartitionUpdateRequest>(
            $"/partitions/{partitionGuid}",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/partitions/{partitionGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }
}
