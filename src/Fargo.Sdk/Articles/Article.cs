namespace Fargo.Api.Articles;

/// <summary>
/// Represents a live article entity. Call <see cref="UpdateAsync"/> to persist property changes.
/// Dispose to unsubscribe from real-time events.
/// </summary>
public sealed class Article : IAsyncDisposable
{
    internal Article(
        Guid guid,
        string name,
        string description,
        IArticleHttpClient client,
        Func<ValueTask>? onDispose = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        ArticleBarcodes? barcodes = null,
        IReadOnlyCollection<Guid>? partitions = null,
        bool isActive = true,
        Guid? editedByGuid = null)
    {
        Guid = guid;
        _name = name;
        _description = description;
        _metrics = metrics;
        _shelfLife = shelfLife;
        _barcodes = barcodes ?? new ArticleBarcodes();
        _partitions = partitions ?? Array.Empty<Guid>();
        _isActive = isActive;
        EditedByGuid = editedByGuid;
        this.client = client;
        _onDispose = onDispose;
    }

    private readonly IArticleHttpClient client;
    private readonly Func<ValueTask>? _onDispose;

    /// <summary>The unique identifier of the article.</summary>
    public Guid Guid { get; }

    /// <summary>The GUID of the user who last edited this article, or <see langword="null"/> if never edited.</summary>
    public Guid? EditedByGuid { get; }

    private string _name;

    /// <summary>The display name of the article.</summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    private string _description;

    /// <summary>The description of the article.</summary>
    public string Description
    {
        get => _description;
        set => _description = value;
    }

    private ArticleMetrics? _metrics;

    /// <summary>
    /// The physical measurements of the article (mass, dimensions, computed density).
    /// <see langword="null"/> when no measurements have been set.
    /// </summary>
    public ArticleMetrics? Metrics
    {
        get => _metrics;
        set => _metrics = value;
    }

    private TimeSpan? _shelfLife;

    /// <summary>
    /// The shelf life of the article, or <see langword="null"/> if no shelf life is defined.
    /// </summary>
    public TimeSpan? ShelfLife
    {
        get => _shelfLife;
        set => _shelfLife = value;
    }

    private ArticleBarcodes _barcodes;

    /// <summary>Gets or sets barcode state for this article, grouped by barcode format.</summary>
    public ArticleBarcodes Barcodes
    {
        get => _barcodes;
        set => _barcodes = value ?? new ArticleBarcodes();
    }

    private IReadOnlyCollection<Guid> _partitions;

    /// <summary>The partitions this article belongs to.</summary>
    public IReadOnlyCollection<Guid> Partitions
    {
        get => _partitions;
        set => _partitions = value ?? Array.Empty<Guid>();
    }

    private bool _isActive;

    /// <summary>Whether this article is active.</summary>
    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    /// <summary>Raised when this article is updated by any authenticated client.</summary>
    public event EventHandler<ArticleUpdatedEventArgs>? Updated;

    /// <summary>Raised when this article is deleted by any authenticated client.</summary>
    public event EventHandler<ArticleDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new ArticleUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new ArticleDeletedEventArgs(Guid));

    /// <summary>
    /// Applies <paramref name="update"/> to this article and persists all changes in a single PUT request.
    /// </summary>
    /// <param name="update">An action that sets one or more properties on this article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the update fails.</exception>
    public async Task UpdateAsync(Action<Article> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var result = await client.UpdateAsync(
            Guid,
            _name,
            _description,
            _partitions,
            _barcodes,
            _metrics,
            _shelfLife,
            _isActive,
            cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!);
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
