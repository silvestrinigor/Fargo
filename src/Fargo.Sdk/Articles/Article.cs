using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Articles;

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
        bool hasImage = false,
        Guid? editedByGuid = null)
    {
        Guid = guid;
        _name = name;
        _description = description;
        _metrics = metrics;
        _shelfLife = shelfLife;
        _hasImage = hasImage;
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

    private bool _hasImage;

    /// <summary>Indicates whether this article has an image stored on the server.</summary>
    public bool HasImage
    {
        get => _hasImage;
        internal set => _hasImage = value;
    }

    /// <summary>Raised when this article is updated by any authenticated client.</summary>
    public event EventHandler<ArticleUpdatedEventArgs>? Updated;

    /// <summary>Raised when this article is deleted by any authenticated client.</summary>
    public event EventHandler<ArticleDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new ArticleUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new ArticleDeletedEventArgs(Guid));

    /// <summary>Gets the partitions that directly contain this article.</summary>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    /// <summary>Adds a partition to this article.</summary>
    public Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
        => client.AddPartitionAsync(Guid, partitionGuid, cancellationToken);

    /// <summary>Removes a partition from this article.</summary>
    public Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
        => client.RemovePartitionAsync(Guid, partitionGuid, cancellationToken);

    /// <summary>
    /// Applies <paramref name="update"/> to this article and persists all changes in a single request.
    /// </summary>
    /// <param name="update">An action that sets one or more properties on this article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the update fails.</exception>
    public async Task UpdateAsync(Action<Article> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var result = await client.UpdateAsync(Guid, _name, _description, _metrics, _shelfLife, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
    }

    /// <summary>
    /// Uploads or replaces the image for this article.
    /// </summary>
    /// <param name="stream">The image data to upload.</param>
    /// <param name="contentType">The MIME type of the image (e.g., <c>image/jpeg</c>).</param>
    /// <param name="fileName">The file name hint sent to the server (e.g., <c>photo.jpg</c>).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the upload fails.</exception>
    public async Task UploadImageAsync(
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default)
    {
        var result = await client.UploadImageAsync(Guid, stream, contentType, fileName, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
        _hasImage = true;
    }

    /// <summary>
    /// Removes the image from this article.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the deletion fails.</exception>
    public async Task DeleteImageAsync(CancellationToken cancellationToken = default)
    {
        var result = await client.DeleteImageAsync(Guid, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
        _hasImage = false;
    }

    /// <summary>
    /// Retrieves the image for this article as a stream.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple of the image <see cref="Stream"/> and its MIME content type,
    /// or <see langword="null"/> if the article has no image.
    /// </returns>
    public Task<(Stream Stream, string ContentType)?> GetImageAsync(CancellationToken cancellationToken = default)
        => client.GetImageAsync(Guid, cancellationToken);

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
