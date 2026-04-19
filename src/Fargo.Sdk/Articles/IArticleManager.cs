namespace Fargo.Sdk.Articles;

/// <summary>
/// Provides high-level operations for managing articles.
/// Returns live <see cref="Article"/> entities whose property setters automatically
/// persist changes to the backend.
/// </summary>
public interface IArticleManager
{
    /// <summary>
    /// Gets a single article by its unique identifier.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="temporalAsOf">When provided, returns the state of the article as it existed at this point in time.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A live <see cref="Article"/> entity.</returns>
    /// <exception cref="FargoSdkApiException">Thrown if the article does not exist or is not accessible.</exception>
    Task<Article> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of articles accessible to the current user.
    /// </summary>
    /// <param name="temporalAsOf">When provided, returns historical data as of this point in time.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of live <see cref="Article"/> entities. Empty if none are found.</returns>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new article and returns it as a live entity.
    /// </summary>
    /// <param name="name">The display name of the article.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="firstPartition">
    /// The partition to associate with the article on creation.
    /// When <see langword="null"/>, the global partition is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created article as a live <see cref="Article"/> entity.</returns>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task<Article> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an article. The article must have no associated items.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the article cannot be deleted or is not accessible.</exception>
    Task DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Raised when any authenticated client creates an article.</summary>
    event EventHandler<ArticleCreatedEventArgs>? Created;

    /// <summary>
    /// Uploads or replaces the image for the specified article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="stream">The image data to upload.</param>
    /// <param name="contentType">The MIME type of the image (e.g., <c>image/jpeg</c>).</param>
    /// <param name="fileName">A file name hint (e.g., <c>photo.jpg</c>).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the image from the specified article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task DeleteImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the image for the specified article as a stream.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple of the image <see cref="Stream"/> and its MIME content type,
    /// or <see langword="null"/> if the article has no image.
    /// </returns>
    /// <exception cref="FargoSdkApiException">Thrown on a server error.</exception>
    Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all barcodes associated with an article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A collection of <see cref="BarcodeResult"/> values. Empty if none exist.</returns>
    /// <exception cref="FargoSdkApiException">Thrown if the article does not exist or on a server error.</exception>
    Task<IReadOnlyCollection<BarcodeResult>> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a barcode to an article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="code">The barcode code string.</param>
    /// <param name="format">The barcode format (symbology).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> of the newly created barcode.</returns>
    /// <exception cref="FargoSdkApiException">Thrown if validation fails, the format already exists, or on a server error.</exception>
    Task<Guid> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a barcode from an article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="barcodeGuid">The unique identifier of the barcode to remove.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the barcode does not exist or on a server error.</exception>
    Task RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default);
}
