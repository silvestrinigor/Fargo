using Fargo.Sdk.Http;
using Fargo.Sdk.Models;

namespace Fargo.Sdk.Items;

internal sealed class ItemClient : IItemClient
{
    private readonly FargoHttpClient httpClient;

    public ItemClient(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<ItemInfo?> GetAsync(Guid itemGuid, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", asOf?.ToString("o")));

        return httpClient.GetFromJsonAsync<ItemInfo>($"/items/{itemGuid}{query}", ct);
    }

    public async Task<IReadOnlyCollection<ItemInfo>> GetManyAsync(Guid? articleGuid = null, DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("articleGuid", articleGuid?.ToString()),
            ("temporalAsOf", asOf?.ToString("o")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<ItemInfo>>($"/items{query}", ct) ?? [];
    }

    public Task<Guid> CreateAsync(Guid articleGuid, Guid? firstPartition = null, CancellationToken ct = default)
    {
        return httpClient.PostFromJsonAsync<object, Guid>(
            "/items",
            new { item = new { articleGuid, firstPartition } },
            ct);
    }

    public Task UpdateAsync(Guid itemGuid, CancellationToken ct = default)
    {
        return httpClient.PatchJsonAsync($"/items/{itemGuid}", new { }, ct);
    }

    public Task DeleteAsync(Guid itemGuid, CancellationToken ct = default)
    {
        return httpClient.DeleteAsync($"/items/{itemGuid}", ct);
    }
}
