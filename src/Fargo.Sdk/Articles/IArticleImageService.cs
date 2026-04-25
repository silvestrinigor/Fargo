namespace Fargo.Sdk.Articles;

/// <summary>Provides image upload, deletion, and retrieval for articles.</summary>
public interface IArticleImageService
{
    /// <summary>Uploads or replaces the image for the specified article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default);

    /// <summary>Removes the image from the specified article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server or access error.</exception>
    Task DeleteImageAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the image for the specified article as a stream.
    /// Returns <see langword="null"/> if the article has no image.
    /// </summary>
    /// <exception cref="FargoSdkApiException">Thrown on a server error.</exception>
    Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}
