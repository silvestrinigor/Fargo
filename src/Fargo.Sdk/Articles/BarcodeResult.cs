namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents a barcode returned by the Fargo API.
/// </summary>
/// <param name="Guid">The unique identifier of the barcode.</param>
/// <param name="ArticleGuid">The unique identifier of the owning article.</param>
/// <param name="Code">The barcode code string.</param>
/// <param name="Format">The barcode format (symbology).</param>
public sealed record BarcodeResult(
    Guid Guid,
    Guid ArticleGuid,
    string Code,
    BarcodeFormat Format);
