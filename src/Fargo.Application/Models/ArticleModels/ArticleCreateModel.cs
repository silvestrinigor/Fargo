using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.ArticleModels
{
    /// <summary>
    /// Represents the data required to create a new article.
    /// </summary>
    /// <param name="Name">
    /// The name of the article.
    /// This value must satisfy the validation rules defined in <see cref="Name"/>.
    /// </param>
    /// <param name="Description">
    /// Optional description associated with the article.
    /// When provided, the value must satisfy the validation rules defined in
    /// <see cref="Description"/>.
    /// </param>
    public record ArticleCreateModel(
            Name Name,
            Description? Description = null
            );
}