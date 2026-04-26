namespace Fargo.Sdk.Articles;

/// <summary>
/// Delegate façade that implements <see cref="IArticleManager"/> by composing the focused services.
/// </summary>
public sealed class ArticleManager(
    IArticleService service,
    IArticleImageService imageService,
    IArticleBarcodeService barcodeService,
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
        Guid? firstPartition = null,
        ArticleMetricsDto? metrics = null,
        TimeSpan? shelfLife = null,
        CancellationToken cancellationToken = default)
        => service.CreateAsync(name, description, firstPartition, metrics, shelfLife, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(articleGuid, cancellationToken);

    /// <inheritdoc />
    public Task UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default)
        => imageService.UploadImageAsync(articleGuid, stream, contentType, fileName, cancellationToken);

    /// <inheritdoc />
    public Task DeleteImageAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => imageService.DeleteImageAsync(articleGuid, cancellationToken);

    /// <inheritdoc />
    public Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
        => imageService.GetImageAsync(articleGuid, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<BarcodeResult>> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
        => barcodeService.GetBarcodesAsync(articleGuid, cancellationToken);

    /// <inheritdoc />
    public Task<Guid> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default)
        => barcodeService.AddBarcodeAsync(articleGuid, code, format, cancellationToken);

    /// <inheritdoc />
    public Task RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default)
        => barcodeService.RemoveBarcodeAsync(articleGuid, barcodeGuid, cancellationToken);
}
