namespace Fargo.Core.Shared.Barcodes;

/// <summary>
/// Defines a strongly typed barcode value that can be converted into a generic
/// <see cref="Barcode"/> representation.
/// </summary>
public interface IBarcodeType
{
    /// <summary>
    /// Gets the symbology represented by this barcode.
    /// </summary>
    BarcodeFormat BarcodeFormat { get; }

    /// <summary>
    /// Converts this barcode value into a generic barcode representation.
    /// </summary>
    /// <returns>
    /// A <see cref="Barcode"/> containing the value and its symbology.
    /// </returns>
    Barcode ToBarcode();
}
