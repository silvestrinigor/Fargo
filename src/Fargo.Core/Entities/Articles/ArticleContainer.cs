using Fargo.Core.Common;
using UnitsNet;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// Constraints on what an <see cref="Article"/> may contain when it is used as a container.
/// </summary>
public sealed class ArticleContainer
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    /// <summary>
    /// <see langword="null"/> when no maximum mass applies.
    /// </summary>
    public Mass? MaxMass { get; private set; }

    /// <summary>
    /// The allow-list of articles, each with an optional maximum quantity.
    /// </summary>
    /// <remarks>
    /// Empty means any article may be inserted (subject to <see cref="MaxMass"/>); otherwise only
    /// the listed articles may be inserted.
    /// </remarks>
    public IReadOnlyCollection<ArticleContainerAllowedArticle> AllowedArticles => allowedArticles;

    private readonly List<ArticleContainerAllowedArticle> allowedArticles = [];

    // Required by EF Core.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleContainer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ArticleContainer(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        Article = article;
        ArticleGuid = article.Guid;
    }

    /// <summary>
    /// Pass <see langword="null"/> to remove the constraint.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// <paramref name="maxMass"/> is less than or equal to zero.
    /// </exception>
    internal void SetMaxMass(Mass? maxMass)
    {
        if (maxMass is not null && maxMass.Value <= Mass.Zero)
        {
            throw new FargoCoreException(
                "The maximum mass of a container must be greater than zero.",
                FargoErrorType.InvalidOperation);
        }

        MaxMass = maxMass;
    }

    /// <summary>
    /// Adds an article to the allow-list. A <paramref name="maxQuantity"/> is only valid for an
    /// entity-tracked article; <see langword="null"/> means no cap.
    /// </summary>
    /// <exception cref="FargoCoreException">
    /// The article is already in the allow-list, the container allows itself, or a maximum
    /// quantity is given for an article that is not entity-tracked.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxQuantity"/> is less than or equal to zero.
    /// </exception>
    internal void AddAllowedArticle(Article allowedArticle, Scalar? maxQuantity)
    {
        ArgumentNullException.ThrowIfNull(allowedArticle);

        if (allowedArticles.Any(a => a.AllowedArticleGuid == allowedArticle.Guid))
        {
            throw new FargoCoreException(
                $"The article '{allowedArticle.Guid}' is already in the container's allowed articles.",
                FargoErrorType.InvalidOperation);
        }

        allowedArticles.Add(new ArticleContainerAllowedArticle(this, allowedArticle, maxQuantity));
    }
}
