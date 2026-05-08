using Fargo.Domain.Barcodes;

namespace Fargo.Application.Articles;

public sealed record ArticleBarcodeDto(string Barcode, BarcodeFormat Type);
