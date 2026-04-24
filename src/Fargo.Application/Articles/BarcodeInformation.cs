using Fargo.Domain.Barcodes;

namespace Fargo.Application.Articles;

public sealed record BarcodeInformation(
    Guid Guid,
    Guid ArticleGuid,
    string Code,
    BarcodeFormat Format);
