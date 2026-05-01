namespace Fargo.Domain.Barcodes;

/// <summary>GS1-128 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Gs1128(string Code);
