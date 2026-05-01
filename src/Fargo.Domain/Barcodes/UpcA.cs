namespace Fargo.Domain.Barcodes;

/// <summary>UPC-A barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct UpcA(string Code);
