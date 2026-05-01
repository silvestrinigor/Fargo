namespace Fargo.Domain.Barcodes;

/// <summary>Data Matrix barcode value assigned to an article.</summary>
/// <param name="Code">The barcode code.</param>
public readonly record struct DataMatrix(string Code);
