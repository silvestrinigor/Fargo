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
            article.ShelfLife,
            article.Color,
            new ArticleMetricsDto(
                article.Mass,
                article.LengthX,
                article.LengthY,
                article.LengthZ),
            new ArticleBarcodesDto(
                article.Barcode.Ean13,
                article.Barcode.Ean8,
                article.Barcode.UpcA,
                article.Barcode.UpcE,
                article.Barcode.Code128,
                article.Barcode.Code39,
                article.Barcode.Itf14,
                article.Barcode.Gs1128,
                article.Barcode.QrCode,
                article.Barcode.DataMatrix),
            article.Partitions.Select(partition => partition.Guid).ToArray());
}
