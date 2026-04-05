using Fargo.Sdk.Http;
using Fargo.Sdk.Models;

namespace Fargo.Sdk.Articles;

internal sealed class ArticleClient : IArticleClient
{
    private readonly FargoHttpClient httpClient;

    public ArticleClient(FargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<ArticleInfo?> GetAsync(Guid articleGuid, DateTimeOffset? asOf = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", asOf?.ToString("o")));

        return httpClient.GetFromJsonAsync<ArticleInfo>($"/articles/{articleGuid}{query}", ct);
    }

    public async Task<IReadOnlyCollection<ArticleInfo>> GetManyAsync(DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("temporalAsOf", asOf?.ToString("o")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        return await httpClient.GetFromJsonAsync<IReadOnlyCollection<ArticleInfo>>($"/articles{query}", ct) ?? [];
    }

    public Task<Guid> CreateAsync(string name, string? description = null, Guid? firstPartition = null, CancellationToken ct = default)
    {
        return httpClient.PostFromJsonAsync<object, Guid>(
            "/articles",
            new { article = new { name, description, firstPartition } },
            ct);
    }

    public Task UpdateAsync(Guid articleGuid, string? name = null, string? description = null, CancellationToken ct = default)
    {
        return httpClient.PatchJsonAsync($"/articles/{articleGuid}", new { name, description }, ct);
    }

    public Task DeleteAsync(Guid articleGuid, CancellationToken ct = default)
    {
        return httpClient.DeleteAsync($"/articles/{articleGuid}", ct);
    }
}
