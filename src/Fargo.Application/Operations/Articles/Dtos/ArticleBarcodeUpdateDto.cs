using Fargo.Core.Barcodes;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <param name="Ean13">New barcode; <see langword="null"/> leaves it unchanged.</param>
/// <param name="RemoveEan13">Clears the barcode instead of changing it.</param>
public sealed record ArticleBarcodeUpdateDto(
    Ean13? Ean13 = null,
    bool RemoveEan13 = false
);
