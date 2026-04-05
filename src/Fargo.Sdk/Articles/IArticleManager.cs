namespace Fargo.Sdk.Articles;

public interface IArticleManager
{
    Task<Article?> GetArticleAsync(Guid articleGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<Article>> GetArticlesAsync(DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateArticleAsync(string name, string? description = null, Guid? firstPartition = null, CancellationToken ct = default);

    Task UpdateArticleAsync(Guid articleGuid, string? name = null, string? description = null, CancellationToken ct = default);

    Task DeleteArticleAsync(Guid articleGuid, CancellationToken ct = default);
}
