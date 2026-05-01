namespace Fargo.Domain.Barcodes;

/// <summary>UPC-E barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcE(string Code);
