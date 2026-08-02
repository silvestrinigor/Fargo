using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleCreateDto(
    Name Name,
    Description? Description = null,
    ArticleType? ArticleType = null,
    Guid? FromArticle = null,
    Scalar? PackQuantity = null,
    IReadOnlyCollection<ArticleKitComponentDto>? KitComponents = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    Mass? Mass = null,
    ArticleDimensionDto? Dimension = null,
    ArticleBarcodeDto? Barcode = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
