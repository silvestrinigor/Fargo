using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents a live article entity. Call <see cref="UpdateAsync"/> to persist property changes.
/// Dispose to unsubscribe from real-time events.
/// </summary>
public sealed class Article : IAsyncDisposable
{
    internal Article(Guid guid, string name, string description, MassDto? mass, IArticleClient client, Func<ValueTask>? onDispose = null,
        LengthDto? lengthX = null, LengthDto? lengthY = null, LengthDto? lengthZ = null, bool hasImage = false)
    {
        Guid = guid;
        _name = name;
        _description = description;
        _mass = mass;
        _lengthX = lengthX;
        _lengthY = lengthY;
        _lengthZ = lengthZ;
        _hasImage = hasImage;
        this.client = client;
        _onDispose = onDispose;
    }

    private readonly IArticleClient client;
    private readonly Func<ValueTask>? _onDispose;

    /// <summary>The unique identifier of the article.</summary>
    public Guid Guid { get; }

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

    private MassDto? _mass;

    /// <summary>
    /// The physical mass of the article.
    /// The value and unit are preserved as returned by the API; no unit conversion is performed.
    /// </summary>
    public MassDto? Mass
    {
        get => _mass;
        set => _mass = value;
    }

    private LengthDto? _lengthX;

    /// <summary>The X dimension of the article.</summary>
    public LengthDto? LengthX
    {
        get => _lengthX;
        set => _lengthX = value;
    }

    private LengthDto? _lengthY;

    /// <summary>The Y dimension of the article.</summary>
    public LengthDto? LengthY
    {
        get => _lengthY;
        set => _lengthY = value;
    }

    private LengthDto? _lengthZ;

    /// <summary>The Z dimension of the article.</summary>
    public LengthDto? LengthZ
    {
        get => _lengthZ;
        set => _lengthZ = value;
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
        var result = await client.UpdateAsync(Guid, _name, _description, _mass, _lengthX, _lengthY, _lengthZ, cancellationToken);
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
