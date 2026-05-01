namespace Fargo.Domain.Barcodes;

/// <summary>EAN-13 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean13(string Code);
