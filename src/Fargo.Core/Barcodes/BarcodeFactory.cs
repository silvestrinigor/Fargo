namespace Fargo.Core.Barcodes;

/// <summary>
/// Provides factory methods for creating barcodes from a format and a raw value.
/// </summary>
public static class BarcodeFactory
{
    /// <summary>
    /// Attempts to create a barcode of the specified format from the given value.
    /// </summary>
    /// <param name="format">The format of the barcode to create.</param>
    /// <param name="value">The raw value of the barcode.</param>
    /// <param name="barcode">
    /// When this method returns, contains the created barcode if the operation
    /// succeeded; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the barcode was created; otherwise,
    /// <see langword="false"/> when the specified format is not supported.
    /// </returns>
    public static bool TryCreate(
        BarcodeFormat format,
        string value,
        out IBarcode barcode)
    {
        switch (format)
        {
            case BarcodeFormat.Ean13:
                barcode = new Ean13(value);
                return true;

            default:
                barcode = null!;
                return false;
        }
    }
}
