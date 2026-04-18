using Fargo.Domain.Barcodes;

namespace Fargo.Application.Models.BarcodeModels;

/// <summary>
/// Represents the data required to add a barcode to an article.
/// </summary>
/// <param name="Code">The barcode code string.</param>
/// <param name="Format">The barcode format (symbology).</param>
public record BarcodeAddModel(string Code, BarcodeFormat Format);
