using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Informations;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the data required to create an article.
/// </summary>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">The optional description of the article.</param>
/// <param name="ArticleType">
/// The type of article to create.
/// </param>
/// <param name="Variation">
/// The optional variation configuration. Required when <paramref name="ArticleType"/>
/// is <see cref="ArticleType.Variation"/>.
/// </param>
/// <param name="Pack">
/// The optional pack configuration. Required when <paramref name="ArticleType"/>
/// is <see cref="ArticleType.Pack"/>.
/// </param>
/// <param name="KitComponents">
/// The optional components of the article. Required when <paramref name="ArticleType"/>
/// is <see cref="ArticleType.Kit"/>.
/// </param>
/// <param name="Container">
/// The optional container configuration when creating a container article.
/// </param>
/// <param name="ShelfLife">The optional shelf life of the article.</param>
/// <param name="Color">The optional color of the article.</param>
/// <param name="Mass">The optional mass of the article.</param>
/// <param name="Dimension">The optional physical dimensions of the article.</param>
/// <param name="Barcode">The optional barcode information of the article.</param>
/// <param name="PartitionsToAdd">
/// The optional identifiers of partitions to associate with the article.
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
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
