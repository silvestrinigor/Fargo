using Fargo.Sdk.Models;

namespace Fargo.Sdk.Articles;

internal interface IArticleClient
{
    Task<ArticleInfo?> GetAsync(Guid articleGuid, DateTimeOffset? asOf = null, CancellationToken ct = default);

    Task<IReadOnlyCollection<ArticleInfo>> GetManyAsync(DateTimeOffset? asOf = null, int? page = null, int? limit = null, CancellationToken ct = default);

    Task<Guid> CreateAsync(string name, string? description = null, Guid? firstPartition = null, CancellationToken ct = default);

    Task UpdateAsync(Guid articleGuid, string? name = null, string? description = null, CancellationToken ct = default);

    Task DeleteAsync(Guid articleGuid, CancellationToken ct = default);
}
