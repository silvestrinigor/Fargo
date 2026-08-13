using Fargo.Core.Barcodes;

namespace Fargo.Application.Articles;

public sealed record ArticleBarcodeUpdateDto(
    Ean13? Ean13 = null,
    bool? RemoveEan13 = null
);
