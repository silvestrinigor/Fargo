using Fargo.Core.Entities.Articles;
using Fargo.Core.Informations;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

/// <param name="Variation">
/// Required when <paramref name="ArticleType"/> is <see cref="ArticleType.Variation"/>.
/// </param>
/// <param name="Pack">
/// Required when <paramref name="ArticleType"/> is <see cref="ArticleType.Pack"/>.
/// </param>
/// <param name="KitComponents">
/// Required when <paramref name="ArticleType"/> is <see cref="ArticleType.Kit"/>.
/// </param>
/// <param name="TrackingMode">
/// Container articles are always entity-tracked regardless of this value.
/// </param>
public sealed record ArticleCreateDto(
    Name Name,
    Description? Description = null,
    ArticleType ArticleType = ArticleType.Default,
    ArticleVariationDto? Variation = null,
    ArticlePackDto? Pack = null,
    IReadOnlyCollection<ArticleKitComponentDto>? KitComponents = null,
    ArticleContainerDto? Container = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    Mass? Mass = null,
    ArticleDimensionDto? Dimension = null,
    ArticleBarcodeDto? Barcode = null,
    ArticleTrackingMode TrackingMode = ArticleTrackingMode.Entity,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
