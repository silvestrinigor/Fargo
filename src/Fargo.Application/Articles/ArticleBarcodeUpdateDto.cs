using Fargo.Core.Shared.Barcodes;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleBarcodeUpdateDto(
    Ean13? Ean13 = null,
    bool? RemoveEan13 = null
);
