namespace Fargo.Domain.Barcodes;

/// <summary>ITF-14 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Itf14(string Code);
