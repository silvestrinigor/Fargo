namespace Fargo.Application.Articles;

/// <summary>
/// Exception thrown when a barcode with the specified identifier cannot be found.
/// </summary>
public class BarcodeNotFoundFargoApplicationException(Guid barcodeGuid)
    : FargoApplicationException($"Barcode with guid '{barcodeGuid}' was not found.")
{
    /// <summary>
    /// Gets the identifier of the barcode that could not be found.
    /// </summary>
    public Guid BarcodeGuid { get; } = barcodeGuid;
}
