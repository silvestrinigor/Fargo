namespace Fargo.Core.Barcodes;

public sealed class BarcodeNone : IBarcode
{
    public override string ToString()
    {
        return "0";
    }
}
