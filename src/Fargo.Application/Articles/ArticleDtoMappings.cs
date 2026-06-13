using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using System.Linq.Expressions;

namespace Fargo.Application.Articles;

/// <summary>
/// Provides mappings for article DTO projections.
/// </summary>
public static class ArticleDtoMappings
{
    /// <summary>
    /// Expression used to project an article entity into an <see cref="ArticleDto"/>.
    /// </summary>
    public static readonly Expression<Func<Article, ArticleDto>> Projection = article => new ArticleDto(
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
            article.Ean13,
            article.Ean8,
            article.UpcA,
            article.UpcE,
            article.Code128,
            article.Code39,
            article.Itf14,
            article.Gs1128,
            article.QrCode,
            article.DataMatrix),
        article.Partitions.Select(partition => partition.Guid).ToArray(),
        article.IsActive,
        article.EditedByActorid
    );
}
