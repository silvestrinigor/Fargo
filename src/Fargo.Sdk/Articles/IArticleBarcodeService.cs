namespace Fargo.Sdk.Articles;

/// <summary>Provides barcode management for articles.</summary>
public interface IArticleBarcodeService
{
    /// <summary>Gets all barcodes associated with an article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the article does not exist or on a server error.</exception>
    Task<ArticleBarcodes> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a barcode to an article.</summary>
    /// <returns>The <see cref="Guid"/> of the newly created barcode.</returns>
    /// <exception cref="FargoSdkApiException">Thrown if validation fails, the format already exists, or on a server error.</exception>
    Task<Guid> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a barcode from an article.</summary>
    /// <exception cref="FargoSdkApiException">Thrown if the barcode does not exist or on a server error.</exception>
    Task RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default);
}
