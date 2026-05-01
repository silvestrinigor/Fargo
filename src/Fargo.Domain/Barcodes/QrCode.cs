namespace Fargo.Domain.Barcodes;

/// <summary>QR Code barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct QrCode(string Code);
