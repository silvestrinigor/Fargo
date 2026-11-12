namespace Fargo.Core.Barcodes;

public static class BarcodeFactory
{
    /// <summary>
    /// Returns <see langword="false"/> only when the format is not supported; an invalid value for a
    /// supported format still throws (see <see cref="Ean13"/>).
    /// </summary>
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
