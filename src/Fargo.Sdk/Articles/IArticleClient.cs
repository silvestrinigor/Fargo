using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Provides operations for managing articles.
/// </summary>
public interface IArticleClient
{
    /// <summary>
    /// Gets a single article by its unique identifier.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="temporalAsOf">
    /// When provided, returns the state of the article as it existed at this point in time.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the <see cref="ArticleResult"/>, or a <see cref="FargoSdkErrorType.NotFound"/>
    /// error if the article does not exist or is not accessible.
    /// </returns>
    Task<FargoSdkResponse<ArticleResult>> GetAsync(
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
    /// <returns>A response containing the collection of articles. The collection is empty if none are found.</returns>
    Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="firstPartition">
    /// The partition to associate with the article on creation.
    /// When <see langword="null"/>, the global partition is used.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the <see cref="Guid"/> of the newly created article.</returns>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing article. Only the properties provided will be changed.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article to update.</param>
    /// <param name="name">The new name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">The new description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        string? name = null,
        string? description = null,
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
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the partitions that directly contain the specified article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the collection of <see cref="PartitionResult"/> values, or a
    /// <see cref="FargoSdkErrorType.NotFound"/> error if the article does not exist.
    /// The collection is empty if the article exists but the current user has no partition access overlap.
    /// </returns>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads or replaces the image for an article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="stream">The image data to upload.</param>
    /// <param name="contentType">The MIME type of the image.</param>
    /// <param name="fileName">A file name hint sent as part of the multipart body.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the image from an article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the image for an article as a stream.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple of the image <see cref="Stream"/> and its MIME content type,
    /// or <see langword="null"/> if the article has no image.
    /// </returns>
    Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}
