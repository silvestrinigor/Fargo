namespace Fargo.Core.Shared.Barcodes;

/// <summary>
/// Represents the barcode format of a barcode.
/// </summary>
public enum BarcodeFormat
{
    /// <summary>
    /// Format not defined.
    /// </summary>
    None = 0,

    /// <summary>
    /// EAN-13 — 13-digit retail barcode.
    /// </summary>
    Ean13 = 1,
}
