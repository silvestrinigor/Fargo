namespace Fargo.Core.Shared.Barcodes;

public sealed class BarcodeNone : IBarcode
{
    public override string ToString()
    {
        return "0";
    }
}
