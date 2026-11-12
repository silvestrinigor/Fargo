using Fargo.Core.Entities.Articles;
using Fargo.Core.Informations;
using System.Drawing;
using System.Linq.Expressions;
using UnitsNet;

namespace Fargo.Application.Dtos.Articles;

/// <param name="Variation"><see langword="null"/> unless the article is a variation.</param>
/// <param name="Pack"><see langword="null"/> unless the article is a pack.</param>
/// <param name="KitComponents"><see langword="null"/> unless the article is a kit.</param>
/// <param name="Container"><see langword="null"/> unless the article is a container.</param>
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

public static class ArticleDtoMapping
{
    /// <summary>
    /// Expression tree, so the query provider can translate it to SQL.
    /// </summary>
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
