namespace Fargo.Core.Barcodes;

/// <summary>
/// Placeholder used when no barcode is assigned; its string form is "0".
/// </summary>
public sealed class BarcodeNone : IBarcode
{
    public override string ToString()
    {
        return "0";
    }
}
