namespace Fargo.Api.Articles;

public interface IArticleManager
{
    Task<Article> GetAsync(Guid articleGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    Task<Article> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default);
}

public sealed class Article
{
    private readonly IArticleHttpClient client;

    internal Article(ArticleResult result, IArticleHttpClient client)
    {
        this.client = client;
        Guid = result.Guid;
        Name = result.Name;
        Description = result.Description;
        Metrics = result.Metrics;
        ShelfLife = result.ShelfLife;
        Barcodes = result.Barcodes;
        Partitions = result.Partitions.ToArray();
        IsActive = result.IsActive;
        EditedByGuid = result.EditedByGuid;
    }

    public Guid Guid { get; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ArticleMetrics? Metrics { get; set; }

    public TimeSpan? ShelfLife { get; set; }

    public ArticleBarcodes Barcodes { get; set; }

    public IReadOnlyCollection<Guid> Partitions { get; set; }

    public bool IsActive { get; set; }

    public Guid? EditedByGuid { get; }

    public async Task UpdateAsync(Action<Article> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var response = await client.UpdateAsync(
            Guid,
            Name,
            Description,
            Partitions,
            Barcodes,
            Metrics,
            ShelfLife,
            IsActive,
            cancellationToken);
        response.EnsureSuccess("Failed to update article.");
    }
}

public sealed class ArticleManager(IArticleHttpClient client) : IArticleManager
{
    public async Task<Article> GetAsync(Guid articleGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => new((await client.GetAsync(articleGuid, temporalAsOf, cancellationToken)).Unwrap("Failed to load article."), client);

    public async Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default)
        => (await client.GetManyAsync(temporalAsOf, page, limit, partitionGuid, search, noPartition, cancellationToken))
            .Unwrap("Failed to load articles.")
            .Select(x => new Article(x, client))
            .ToArray();

    public async Task<Article> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var guid = (await client.CreateAsync(name, description, partitions, barcodes, metrics, shelfLife, isActive, cancellationToken))
            .Unwrap("Failed to create article.");

        return await GetAsync(guid, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => (await client.DeleteAsync(articleGuid, cancellationToken)).EnsureSuccess("Failed to delete article.");
}
