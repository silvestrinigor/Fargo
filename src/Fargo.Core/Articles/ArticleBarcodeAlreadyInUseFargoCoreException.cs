using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Exception thrown when a barcode is already assigned to a different article.
/// </summary>
public sealed class ArticleBarcodeAlreadyInUseFargoCoreException(Barcode barcode)
    : FargoCoreException(
        $"Barcode '{barcode}' is already assigned to another article.",
        FargoCoreErrorType.ArticleBarcodeAlreadyInUse)
{
    public Barcode Barcode { get; } = barcode;
}
