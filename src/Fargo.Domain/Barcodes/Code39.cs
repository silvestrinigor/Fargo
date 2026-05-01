namespace Fargo.Domain.Barcodes;

/// <summary>Code 39 barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct Code39(string Code);
