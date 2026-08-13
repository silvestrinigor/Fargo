using Fargo.Core.Shared.Barcodes;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleBarcodeDto(
    Ean13? Ean13 = null
);
