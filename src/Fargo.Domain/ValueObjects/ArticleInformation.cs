namespace Fargo.Domain.ValueObjects
{
    /// <summary>
    /// Represents a lightweight information projection of an Article entity.
    /// </summary>
    /// <remarks>
    /// This value object is typically used when only basic article information
    /// is required, avoiding the need to load the full <c>Article</c> aggregate.
    /// It is commonly used for listings, references, or nested projections
    /// inside other domain models.
    /// </remarks>
    /// <param name="Guid">
    /// The unique identifier of the article.
    /// </param>
    /// <param name="Name">
    /// The name or title of the article.
    /// </param>
    /// <param name="Description">
    /// A short description summarizing the article content.
    /// </param>
    public sealed record ArticleInformation(
        Guid Guid,
        Name Name,
        Description Description
    );
}