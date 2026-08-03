using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Informations;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleCreateDto(
    Name Name,
    Description? Description = null,
    ArticleType? ArticleType = null,
    ArticleVariationDto? Variation = null,
    ArticlePackDto? Pack = null,
    IReadOnlyCollection<ArticleKitComponentDto>? KitComponents = null,
    ArticleContainerDto? Container = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    Mass? Mass = null,
    ArticleDimensionDto? Dimension = null,
    ArticleBarcodeDto? Barcode = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
