namespace Fargo.Api.Articles;

/// <summary>
/// Delegate façade that implements <see cref="IArticleManager"/> by composing the focused services.
/// </summary>
public sealed class ArticleManager(
    IArticleService service,
    IArticleEventSource eventSource) : IArticleManager
{
    /// <inheritdoc />
    public event EventHandler<ArticleCreatedEventArgs>? Created
    {
        add => eventSource.Created += value;
        remove => eventSource.Created -= value;
    }

    /// <inheritdoc />
    public Task<Article> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => service.GetAsync(articleGuid, temporalAsOf, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default)
        => service.GetManyAsync(temporalAsOf, page, limit, partitionGuid, search, noPartition, cancellationToken);

    /// <inheritdoc />
    public Task<Article> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
        => service.CreateAsync(name, description, partitions, barcodes, metrics, shelfLife, isActive, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(articleGuid, cancellationToken);
}
