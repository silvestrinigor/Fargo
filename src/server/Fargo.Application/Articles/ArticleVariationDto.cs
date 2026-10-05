namespace Fargo.Application.Articles;

/// <summary>
/// Represents the variation information of an article.
/// </summary>
/// <param name="FromArticleGuid">
/// The unique identifier of the article from which the variation was created.
/// </param>
public sealed record ArticleVariationDto(
    Guid FromArticleGuid
);
