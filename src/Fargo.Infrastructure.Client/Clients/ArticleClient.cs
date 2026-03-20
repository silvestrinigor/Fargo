using Fargo.Application.Models.ArticleModels;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Http;

namespace Fargo.Infrastructure.Client.Clients;

public sealed class ArticleClient(HttpClient http)
    : FargoHttpClientBase(http), IArticleClient
{
    public Task<ArticleInformation?> GetSingleAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/articles/{articleGuid}?temporalAsOf={temporalAsOf}";
        return GetAsync<ArticleInformation?>(uri, cancellationToken);
    }

    public Task<IReadOnlyCollection<ArticleInformation>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"/articles?temporalAsOf={temporalAsOf}&page={page}&limit={limit}";
        return GetCollectionAsync<ArticleInformation>(uri, cancellationToken);
    }

    public Task<Guid> CreateAsync(
        ArticleCreateModel model,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<Guid>("/articles", model, cancellationToken);
    }

    public Task UpdateAsync(
        Guid articleGuid,
        ArticleUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync($"/articles/{articleGuid}", model, cancellationToken);
    }

    public Task DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync($"/articles/{articleGuid}", cancellationToken);
    }
}
