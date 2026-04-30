using Fargo.Domain.Articles;
using System.Linq.Expressions;

namespace Fargo.Application.Articles;

/// <summary>
/// Provides mapping utilities for transforming <see cref="Article"/> entities
/// into lightweight data transfer representations.
/// </summary>
/// <remarks>
/// This class centralizes projection logic for <see cref="Article"/> to ensure
/// consistency across queries and in-memory transformations.
///
/// Two mapping approaches are provided:
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="InformationProjection"/>: An expression-based projection that can be
/// translated by LINQ providers (e.g., Entity Framework Core) into SQL queries,
/// allowing efficient data retrieval without loading full entities.
/// </description>
/// </item>
/// <item>
/// <description>
/// <see cref="ToInformation(Article)"/>: An in-memory mapping method used when
/// the entity has already been materialized.
/// </description>
/// </item>
/// </list>
/// </remarks>
public static class ArticleMappings
{
    /// <summary>
    /// Gets an expression used to project an <see cref="Article"/> entity
    /// into an <see cref="ArticleInformation"/> object.
    /// </summary>
    /// <remarks>
    /// This projection is intended for use in LINQ queries executed by a query provider
    /// such as Entity Framework Core. Because it is an expression tree, it can be
    /// translated into SQL, allowing only the required fields to be selected from
    /// the database.
    ///
    /// Using this projection avoids materializing the full <see cref="Article"/> entity,
    /// improving performance for read operations.
    /// </remarks>
    public static readonly Expression<Func<Article, ArticleInformation>> InformationProjection =
        a => new ArticleInformation(
            a.Guid,
            a.Name,
            a.Description,
            a.Metrics,
            a.ShelfLife,
            a.Images.HasImage,
            a.EditedByGuid,
            new ArticleImages(a.Images.HasImage)
        );

    /// <summary>
    /// Maps an <see cref="Article"/> entity to an <see cref="ArticleInformation"/> object.
    /// </summary>
    /// <param name="a">The <see cref="Article"/> to map.</param>
    /// <returns>
    /// A new <see cref="ArticleInformation"/> instance containing the mapped data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="a"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method performs an in-memory transformation and should be used only when
    /// the <see cref="Article"/> entity has already been loaded into memory.
    ///
    /// For queryable scenarios (e.g., database queries), prefer using
    /// <see cref="InformationProjection"/> to ensure efficient execution.
    /// </remarks>
    public static ArticleInformation ToInformation(this Article a)
    {
        ArgumentNullException.ThrowIfNull(a);

        return new ArticleInformation(
            a.Guid,
            a.Name,
            a.Description,
            a.Metrics,
            a.ShelfLife,
            a.Images.HasImage,
            a.EditedByGuid,
            new ArticleImages(a.Images.HasImage)
        );
    }
}
