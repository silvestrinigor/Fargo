using Fargo.Core.Articles;
using Fargo.Core.Informations;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the application-layer data transfer object for an article.
/// </summary>
/// <param name="Guid">The unique identifier of the article.</param>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">The description of the article.</param>
/// <param name="ArticleType">The type of the article.</param>
/// <param name="ShelfLife">The optional shelf life of the article.</param>
/// <param name="Color">The optional color of the article.</param>
/// <param name="Mass">The optional mass of the article.</param>
/// <param name="Dimension">The physical dimensions of the article.</param>
/// <param name="Variation">The variation information when the article is a variation; otherwise, <see langword="null"/>.</param>
/// <param name="Pack">The packaging information when the article is a pack; otherwise, <see langword="null"/>.</param>
/// <param name="KitComponents">The components that compose the article when it is a kit; otherwise, <see langword="null"/>.</param>
/// <param name="Container">The container information when the article is a container; otherwise, <see langword="null"/>.</param>
/// <param name="Barcodes">The barcodes associated with the article.</param>
/// <param name="Partitions">The identifiers of the partitions associated with the article.</param>
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
