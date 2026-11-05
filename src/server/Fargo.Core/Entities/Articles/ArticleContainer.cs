using Fargo.Core.Common;
using UnitsNet;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// Defines container constraints for an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// A container article represents an article that may contain other articles,
/// optionally constrained by allowed articles, restricted articles, and maximum mass.
/// </remarks>
public sealed class ArticleContainer
{
    /// <summary>
    /// Gets the unique identifier of the associated article.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article associated with these container constraints.
    /// </summary>
    public Article Article { get; private init; }

    /// <summary>
    /// Gets or sets the maximum mass allowed inside the container.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, no maximum mass constraint is defined.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to zero.
    /// </exception>
    public Mass? MaxMass { get; private set; }

    /// <summary>
    /// Gets the articles that are allowed to be inserted into the container,
    /// together with the maximum quantity of each.
    /// </summary>
    /// <remarks>
    /// When the collection is empty, the container imposes no allow-list
    /// constraint and may hold any article (subject to other constraints such
    /// as <see cref="MaxMass"/>). When the collection is non-empty, only the
    /// listed articles may be inserted, each bounded by its maximum quantity.
    /// </remarks>
    public IReadOnlyCollection<ArticleContainerAllowedArticle> AllowedArticles => allowedArticles;

    private readonly List<ArticleContainerAllowedArticle> allowedArticles = [];

    /// <summary>
    /// Initializes a new <see cref="ArticleContainer"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleContainer() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes the container constraints for the specified article.
    /// </summary>
    /// <param name="article">
    /// The container article.
    /// </param>
    internal ArticleContainer(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        Article = article;
        ArticleGuid = article.Guid;
    }

    /// <summary>
    /// Sets the maximum mass allowed inside the container.
    /// </summary>
    /// <param name="maxMass">
    /// The maximum allowed mass, or <see langword="null"/> to remove the constraint.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxMass"/> is less than or equal to zero.
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
    /// Adds an article to the container's allow-list, optionally constrained by
    /// a maximum quantity.
    /// </summary>
    /// <param name="allowedArticle">
    /// The article allowed inside the container.
    /// </param>
    /// <param name="maxQuantity">
    /// The maximum quantity of the allowed article the container may hold, or
    /// <see langword="null"/> to impose no quantity cap. A maximum quantity may
    /// only be supplied for an entity-tracked allowed article.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="allowedArticle"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the container allows itself, when the article has already
    /// been added to the allow-list, or when a maximum quantity is supplied for
    /// an allowed article that is not entity-tracked.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxQuantity"/> is less than or equal to zero.
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
