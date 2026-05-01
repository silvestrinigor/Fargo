namespace Fargo.Domain.Barcodes;

/// <summary>Code 128 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code128(string Code);
