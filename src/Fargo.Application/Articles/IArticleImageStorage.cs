namespace Fargo.Application.Storage;

/// <summary>
/// Abstraction for article image storage.
/// Implementations may store images on the local file system, in an S3-compatible
/// object store (e.g., AWS S3, MinIO), or any other backend.
/// </summary>
/// <remarks>
/// The <em>key</em> returned by <see cref="SaveAsync"/> is a provider-agnostic
/// identifier that is persisted in the database. Swapping the implementation
/// (e.g., from local disk to S3) only requires re-registering the service in the
/// DI container — no domain or application code changes are needed.
/// </remarks>
public interface IArticleImageStorage
{
    /// <summary>
    /// Persists an image for the specified article and returns its storage key.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article that owns the image.</param>
    /// <param name="stream">The image data to store.</param>
    /// <param name="contentType">The MIME type of the image (e.g., <c>image/jpeg</c>).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A provider-agnostic key that uniquely identifies the stored image.
    /// This value should be saved to <see cref="Fargo.Domain.Articles.Article.ImageKey"/>.
    /// </returns>
    Task<string> SaveAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an image by its storage key.
    /// </summary>
    /// <param name="key">The key previously returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A tuple of the image <see cref="Stream"/> and its MIME content type,
    /// or <see langword="null"/> if no image exists for the given key.
    /// </returns>
    Task<(Stream Stream, string ContentType)?> GetAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the image identified by the given key.
    /// Implementations must be idempotent: deleting a non-existent key must not throw.
    /// </summary>
    /// <param name="key">The key previously returned by <see cref="SaveAsync"/>.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default);
}
