using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleDto(
    Guid Guid,
    Name Name,
    Description Description,
    ArticleType ArticleType,
    TimeSpan? ShelfLife,
    Color? Color,
    Mass? Mass,
    ArticleDimensionDto Dimension,
    ArticleVariationDto? Variation,
    ArticlePackDto? Pack,
    IReadOnlyCollection<ArticleKitComponentDto>? KitComponents,
    ArticleContainerDto? Container,
    ArticleBarcodeDto Barcodes,
    IReadOnlyCollection<Guid> Partitions
);
