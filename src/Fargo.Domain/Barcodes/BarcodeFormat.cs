namespace Fargo.Domain.Barcodes;

// TODO: Move to a new project Fargo.Types to remove the duplicated code in the sdk.
/// <summary>
/// Represents the barcode format (symbology) of a barcode.
/// </summary>
public enum BarcodeFormat
{
    /// <summary>EAN-13 — 13-digit retail barcode.</summary>
    Ean13 = 0,

    /// <summary>EAN-8 — 8-digit compact retail barcode.</summary>
    Ean8 = 1,

    /// <summary>UPC-A — 12-digit North American retail barcode.</summary>
    UpcA = 2,

    /// <summary>UPC-E — 8-digit compressed UPC barcode.</summary>
    UpcE = 3,

    /// <summary>Code 128 — variable-length alphanumeric barcode widely used in logistics.</summary>
    Code128 = 4,

    /// <summary>Code 39 — variable-length alphanumeric barcode used in industrial and military contexts.</summary>
    Code39 = 5,

    /// <summary>ITF-14 — 14-digit barcode used on outer packaging and pallets.</summary>
    Itf14 = 6,

    /// <summary>GS1-128 — Code 128 with GS1 Application Identifiers for supply chain.</summary>
    Gs1128 = 7,

    /// <summary>QR Code — 2D matrix barcode for URLs and structured data.</summary>
    QrCode = 8,

    /// <summary>Data Matrix — compact 2D matrix barcode for small item labelling.</summary>
    DataMatrix = 9,
}
