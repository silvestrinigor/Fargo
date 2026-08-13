using Fargo.Core.Barcodes;

namespace Fargo.Application.Articles;

public sealed record ArticleBarcodeDto(
    Ean13? Ean13 = null
);
