using Fargo.Sdk.Http;
using Fargo.Sdk.Models;

namespace Fargo.Sdk.Managers;

internal sealed class PartitionManager : IPartitionManager
{
    private readonly FargoHttpClient httpClient;

    public PartitionManager(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<PartitionInfo?> GetAsync(Guid partitionGuid, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", asOf?.ToString("o")));

        return httpClient.GetFromJsonAsync<PartitionInfo>($"/partitions/{partitionGuid}{query}", ct);
    }

    public async Task<IReadOnlyCollection<PartitionInfo>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("parentPartitionGuid", parentPartitionGuid?.ToString()),
            ("temporalAsOf", asOf?.ToString("o")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<PartitionInfo>>($"/partitions{query}", ct) ?? [];
    }

    public Task<Guid> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken ct = default)
    {
        return httpClient.PostFromJsonAsync<object, Guid>(
            "/partitions",
            new { name, description, parentPartitionGuid },
            ct);
    }

    public Task UpdateAsync(Guid partitionGuid, string? description = null, Guid? parentPartitionGuid = null, CancellationToken ct = default)
    {
        return httpClient.PatchJsonAsync(
            $"/partitions/{partitionGuid}",
            new { description, parentPartitionGuid },
            ct);
    }

    public Task DeleteAsync(Guid partitionGuid, CancellationToken ct = default)
    {
        return httpClient.DeleteAsync($"/partitions/{partitionGuid}", ct);
    }
}
