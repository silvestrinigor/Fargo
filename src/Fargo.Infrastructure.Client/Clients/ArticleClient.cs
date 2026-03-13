using Fargo.Application.Common;
using Fargo.Application.Models.ArticleModels;
using Fargo.HttpApi.Client.Interfaces;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class ArticleClient(HttpClient http)
    : FargoHttpClientBase(http), IArticleClient
{
    public Task<ArticleReadModel?> GetSingleAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
    {
        var uri = $"/articles/{articleGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<ArticleReadModel>(uri, ct);
    }

    public Task<IReadOnlyCollection<ArticleReadModel>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri = $"/articles?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";
        return GetCollectionAsync<ArticleReadModel>(uri, ct);
    }

    public Task<Guid> CreateAsync(ArticleCreateModel model, CancellationToken ct = default)
        => PostAsync<Guid>("/articles", model, ct);

    public Task UpdateAsync(Guid articleGuid, ArticleUpdateModel model, CancellationToken ct = default)
        => PatchAsync($"/articles/{articleGuid}", model, ct);

    public Task DeleteAsync(Guid articleGuid, CancellationToken ct = default)
        => DeleteAsync($"/articles/{articleGuid}", ct);
}