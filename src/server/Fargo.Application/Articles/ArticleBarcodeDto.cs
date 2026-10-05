using Fargo.Core.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the barcode information associated with an article.
/// </summary>
/// <param name="Ean13">The optional EAN-13 barcode of the article.</param>
public sealed record ArticleBarcodeDto(
    Ean13? Ean13 = null
);
