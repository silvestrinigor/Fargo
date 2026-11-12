using Fargo.Core.Barcodes;

namespace Fargo.Application.Operations.Articles.Dtos;

public sealed record ArticleBarcodeDto(
    Ean13? Ean13 = null
);
