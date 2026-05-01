namespace Fargo.Api.Articles;

/// <summary>Provides barcode management for articles.</summary>
public interface IArticleBarcodeService
{
    /// <summary>Gets all barcodes associated with an article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the article does not exist or on a server error.</exception>
    Task<ArticleBarcodes> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces all barcodes associated with an article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if validation fails or on a server error.</exception>
    Task UpdateBarcodesAsync(
        Guid articleGuid,
        ArticleBarcodes barcodes,
        CancellationToken cancellationToken = default);
}
