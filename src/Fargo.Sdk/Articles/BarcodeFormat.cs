namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents the barcode format (symbology) of a barcode.
/// Must mirror <c>Fargo.Domain.Enums.BarcodeFormat</c> integer-for-integer.
/// </summary>
public enum BarcodeFormat
{
    /// <summary>EAN-13 — 13-digit retail barcode.</summary>
    Ean13,

    /// <summary>EAN-8 — 8-digit compact retail barcode.</summary>
    Ean8,

    /// <summary>UPC-A — 12-digit North American retail barcode.</summary>
    UpcA,

    /// <summary>UPC-E — 8-digit compressed UPC barcode.</summary>
    UpcE,

    /// <summary>Code 128 — variable-length alphanumeric barcode widely used in logistics.</summary>
    Code128,

    /// <summary>Code 39 — variable-length alphanumeric barcode used in industrial and military contexts.</summary>
    Code39,

    /// <summary>ITF-14 — 14-digit barcode used on outer packaging and pallets.</summary>
    Itf14,

    /// <summary>GS1-128 — Code 128 with GS1 Application Identifiers for supply chain.</summary>
    Gs1128,

    /// <summary>QR Code — 2D matrix barcode for URLs and structured data.</summary>
    QrCode,

    /// <summary>Data Matrix — compact 2D matrix barcode for small item labelling.</summary>
    DataMatrix,
}
