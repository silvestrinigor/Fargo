using Fargo.Core.Entities.Articles;
using Fargo.Core.Informations;
using System.Drawing;
using UnitsNet;
using System.Linq.Expressions;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <summary>
/// Represents the application-layer data transfer object for an article.
/// </summary>
/// <param name="Guid">The unique identifier of the article.</param>
/// <param name="Name">The name of the article.</param>
/// <param name="Description">The description of the article.</param>
/// <param name="ArticleType">The type of the article.</param>
/// <param name="TrackingMode">The stock tracking mode of the article (entity or event).</param>
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
    ArticleTrackingMode TrackingMode,
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

/// <summary>
/// Provides mappings from article domain entities to application-layer DTOs.
/// </summary>
public static class ArticleDtoMapping
{
    /// <summary>
    /// Gets an expression that projects an <see cref="Article"/> into an <see cref="ArticleDto"/>.
    /// </summary>
    /// <remarks>
    /// The projection is represented as an expression tree so it can be translated
    /// and executed by the underlying query provider.
    /// </remarks>
    public static readonly Expression<Func<Article, ArticleDto>> Projection = article
        => new ArticleDto(
            article.Guid,
            article.Name,
            article.Description,
            article.ArticleType,
            article.TrackingMode,
            article.ShelfLife,
            article.Color,
            article.Mass,
            Dimension: new ArticleDimensionDto(
                article.Dimension.X,
                article.Dimension.Y,
                article.Dimension.Z),
            article.Variation != null
                ? new ArticleVariationDto(
                article.Variation.FromArticleGuid)
                : null,
            article.Pack != null
                ? new ArticlePackDto(
                    article.Pack.FromArticleGuid,
                    article.Pack.Quantity)
                : null,
            article.ArticleType == ArticleType.Kit
                ? article.KitComponents
                    .Select(k => new ArticleKitComponentDto(k.FromArticleGuid, k.Quantity)).ToArray()
                : null,
            article.Container != null
                ? new ArticleContainerDto(
                    article.Container.MaxMass,
                    article.Container.AllowedArticles
                        .Select(a => new ArticleContainerAllowedArticleDto(a.AllowedArticleGuid, a.MaxQuantity))
                        .ToArray())
                : null,
            Barcodes: new ArticleBarcodeDto(
                article.Barcode.Ean13),
            Partitions: article.Partitions.Select(partition => partition.PartitionGuid).ToArray()
        );
}
