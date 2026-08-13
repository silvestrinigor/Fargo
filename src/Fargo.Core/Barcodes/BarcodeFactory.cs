namespace Fargo.Core.Barcodes;

public static class BarcodeFactory
{
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
