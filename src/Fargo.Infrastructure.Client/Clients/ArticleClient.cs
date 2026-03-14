using Fargo.Application.Models.ArticleModels;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class ArticleClient(HttpClient http)
    : FargoHttpClientBase(http), IArticleClient
{
    public Task<ArticleInformation?> GetSingleAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken ct = default)
    {
        var uri = $"/articles/{articleGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<ArticleInformation>(uri, ct);
    }

    public Task<IReadOnlyCollection<ArticleInformation>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken ct = default)
    {
        var uri = $"/articles?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";
        return GetCollectionAsync<ArticleInformation>(uri, ct);
    }

    public Task<Guid> CreateAsync(ArticleCreateModel model, CancellationToken ct = default)
        => PostAsync<Guid>("/articles", model, ct);

    public Task UpdateAsync(Guid articleGuid, ArticleUpdateModel model, CancellationToken ct = default)
        => PatchAsync($"/articles/{articleGuid}", model, ct);

    public Task DeleteAsync(Guid articleGuid, CancellationToken ct = default)
        => DeleteAsync($"/articles/{articleGuid}", ct);
}