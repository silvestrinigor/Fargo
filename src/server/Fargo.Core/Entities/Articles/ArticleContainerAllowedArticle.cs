using Fargo.Core.Common;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// Defines one article that is allowed to be inserted into a container article,
/// optionally constrained by the maximum quantity of that article the container
/// may hold.
/// </summary>
/// <remarks>
/// When a container defines one or more allowed articles, only those articles
/// may be inserted into the container. Each allowed article may further be
/// bounded by a <see cref="MaxQuantity"/>; when <see cref="MaxQuantity"/> is
/// <see langword="null"/>, no quantity cap applies.
///
/// A maximum quantity may only be defined for an entity-tracked allowed article
/// (<see cref="ArticleTrackingMode.Entity"/>): event-tracked articles record
/// stock as aggregate movement events rather than as individually placed items,
/// so a per-item quantity cap is not meaningful for them.
/// </remarks>
public sealed class ArticleContainerAllowedArticle
{
    /// <summary>
    /// Gets the unique identifier of the container article that owns this
    /// allowance.
    /// </summary>
    public Guid ContainerArticleGuid { get; private init; }

    /// <summary>
    /// Gets the container that owns this allowance.
    /// </summary>
    public ArticleContainer Container { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the article allowed inside the container.
    /// </summary>
    public Guid AllowedArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article allowed inside the container.
    /// </summary>
    public Article AllowedArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the maximum quantity of the allowed article the container may hold,
    /// or <see langword="null"/> when no quantity cap applies.
    /// </summary>
    public Scalar? MaxQuantity { get; private init; }

    /// <summary>
    /// Initializes a new <see cref="ArticleContainerAllowedArticle"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ArticleContainerAllowedArticle()
    {
    }

    /// <summary>
    /// Initializes a new allowed-article constraint for a container.
    /// </summary>
    /// <param name="container">
    /// The container that allows the article.
    /// </param>
    /// <param name="allowedArticle">
    /// The article allowed inside the container.
    /// </param>
    /// <param name="maxQuantity">
    /// The maximum quantity of the allowed article the container may hold, or
    /// <see langword="null"/> to impose no quantity cap.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="container"/> or <paramref name="allowedArticle"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FargoCoreException">
    /// Thrown when the container allows itself, or when a maximum quantity is
    /// supplied for an allowed article that is not entity-tracked.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxQuantity"/> is less than or equal to zero.
    /// </exception>
    internal ArticleContainerAllowedArticle(ArticleContainer container, Article allowedArticle, Scalar? maxQuantity)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(allowedArticle);

        if (container.ArticleGuid == allowedArticle.Guid)
        {
            throw new FargoCoreException(
                "A container cannot allow itself as one of its contents.",
                FargoErrorType.InvalidOperation);
        }

        Container = container;
        ContainerArticleGuid = container.ArticleGuid;

        AllowedArticle = allowedArticle;
        AllowedArticleGuid = allowedArticle.Guid;

        ValidateMaxQuantity(allowedArticle, maxQuantity);

        MaxQuantity = maxQuantity;
    }

    private static void ValidateMaxQuantity(Article allowedArticle, Scalar? maxQuantity)
    {
        if (maxQuantity is null)
        {
            return;
        }

        if (allowedArticle.TrackingMode != ArticleTrackingMode.Entity)
        {
            throw new FargoCoreException(
                $"A maximum quantity can only be defined for the entity-tracked allowed article; article '{allowedArticle.Guid}' is not entity-tracked.",
                FargoErrorType.InvalidOperation);
        }

        if (maxQuantity.Value <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxQuantity),
                maxQuantity,
                "A container allowed-article maximum quantity must be greater than zero.");
        }
    }
}
