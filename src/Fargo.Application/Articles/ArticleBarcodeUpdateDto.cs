using Fargo.Core.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the barcode changes to apply to an article.
/// </summary>
/// <param name="Ean13">
/// The new EAN-13 barcode to assign to the article. A <see langword="null"/> value
/// leaves the existing EAN-13 barcode unchanged.
/// </param>
/// <param name="RemoveEan13">
/// Indicates whether the existing EAN-13 barcode should be removed by setting its
/// value to <see langword="null"/>.
/// </param>
public sealed record ArticleBarcodeUpdateDto(
    Ean13? Ean13 = null,
    bool RemoveEan13 = false
);
