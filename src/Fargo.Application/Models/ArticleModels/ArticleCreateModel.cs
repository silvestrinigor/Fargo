using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    /// <summary>
    /// Represents the data required to create a new article.
    /// </summary>
    /// <param name="Name">
    /// The name of the article.
    /// </param>
    /// <param name="Description">
    /// Optional description of the article.
    /// </param>
    public record ArticleCreateModel(
            Name Name,
            Description? Description = null
            );
}