namespace Fargo.Domain.Barcodes;

/// <summary>
/// A read-only projection of a <see cref="Barcode"/> entity,
/// used in query results to avoid exposing domain entities to callers.
/// </summary>
/// <param name="Guid">The unique identifier of the barcode.</param>
/// <param name="ArticleGuid">The unique identifier of the owning article.</param>
/// <param name="Code">The barcode code string.</param>
/// <param name="Format">The barcode format (symbology).</param>
public sealed record BarcodeInformation(
    Guid Guid,
    Guid ArticleGuid,
    string Code,
    BarcodeFormat Format);
