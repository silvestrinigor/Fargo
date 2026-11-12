using Fargo.Core.Common;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// One article allowed inside a container article, optionally capped by a maximum quantity.
/// </summary>
/// <remarks>
/// A maximum quantity is only valid for an entity-tracked allowed article
/// (<see cref="ArticleTrackingMode.Entity"/>): event-tracked articles record stock as aggregate
/// movement events, so a per-item cap is not meaningful for them.
/// </remarks>
public sealed class ArticleContainerAllowedArticle
{
    public Guid ContainerArticleGuid { get; private init; }

    public ArticleContainer Container { get; private init; } = null!;

    public Guid AllowedArticleGuid { get; private init; }

    public Article AllowedArticle { get; private init; } = null!;

    /// <summary>
    /// <see langword="null"/> when no quantity cap applies.
    /// </summary>
    public Scalar? MaxQuantity { get; private init; }

    // Required by EF Core.
    private ArticleContainerAllowedArticle()
    {
    }

    /// <exception cref="FargoCoreException">
    /// The container allows itself, or a maximum quantity is given for an article that is not
    /// entity-tracked.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxQuantity"/> is less than or equal to zero.
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
