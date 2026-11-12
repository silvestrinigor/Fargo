namespace Fargo.Core.Barcodes;

/// <summary>
/// Represents an empty or unspecified barcode.
/// </summary>
/// <remarks>
/// Used when no barcode is assigned. Its string representation is <c>"0"</c>.
/// </remarks>
public sealed class BarcodeNone : IBarcode
{
    /// <summary>
    /// Returns the string representation of the empty barcode.
    /// </summary>
    /// <returns>The string <c>"0"</c>.</returns>
    public override string ToString()
    {
        return "0";
    }
}
