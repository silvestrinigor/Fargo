namespace Fargo.Domain.Barcodes;

/// <summary>EAN-8 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Ean8(string Code);
