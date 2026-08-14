using Fargo.Core.Articles;
using System.Linq.Expressions;

namespace Fargo.Application.Articles;

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
            article.ShelfLife,
            article.Color,
            article.Mass,
            new ArticleDimensionDto(
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
                ? new ArticleContainerDto(article.Container.MaxMass)
                : null,
            new ArticleBarcodeDto(
                article.Barcode.Ean13),
            article.Partitions.Select(partition => partition.PartitionGuid).ToArray());
}
