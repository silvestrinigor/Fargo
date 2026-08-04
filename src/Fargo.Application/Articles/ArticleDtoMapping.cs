using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using System.Linq.Expressions;

namespace Fargo.Application.Articles;

public static class ArticleDtoMapping
{
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
            article.KitComponents != null
                ? article.KitComponents
                    .Select(k => new ArticleKitComponentDto(k.FromArticleGuid, k.Quantity)).ToArray()
                : null,
            article.Container != null
                ? new ArticleContainerDto(article.Container.MaxMass)
                : null,
            new ArticleBarcodeDto(
                article.Barcode.Ean13),
            article.Partitions.Select(partition => partition.Guid).ToArray());
}
