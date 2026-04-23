namespace Fargo.Domain.Articles;

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
/// <param name="Mass">
/// The physical mass of the article, or <see langword="null"/> if not specified.
/// </param>
/// <param name="LengthX">
/// The X dimension of the article, or <see langword="null"/> if not specified.
/// </param>
/// <param name="LengthY">
/// The Y dimension of the article, or <see langword="null"/> if not specified.
/// </param>
/// <param name="LengthZ">
/// The Z dimension of the article, or <see langword="null"/> if not specified.
/// </param>
/// <param name="HasImage">
/// Indicates whether the article has an image stored.
/// </param>
/// <param name="EditedByGuid">
/// The unique identifier of the actor who last edited the article,
/// or <see langword="null"/> if not provided.
/// </param>
public sealed record ArticleInformation(
    Guid Guid,
    Name Name,
    Description Description,
    Mass? Mass,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null,
    bool HasImage = false,
    Guid? EditedByGuid = null
);
