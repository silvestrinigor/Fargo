using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines an article pack relationship.
/// </summary>
/// <remarks>
/// A pack article represents a quantity of another article grouped as a single article.
/// For example, a pack may represent twelve units of the same source article.
/// </remarks>
public sealed class ArticlePack
{
    private ArticlePack()
    {
    }

    public ArticlePack(Article fromArticle, Scalar quantity)
    {
        FromArticle = fromArticle;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Gets the unique identifier of the source article contained in the pack.
    /// </summary>
    public Guid FromArticleGuid { get; private set; }

    /// <summary>
    /// Gets the article from which this pack is composed.
    /// </summary>
    public Article FromArticle
    {
        get;
        private set
        {
            FromArticleGuid = value.Guid;
            field = value;
        }
    } = null!;

    /// <summary>
    /// Gets or sets the quantity of the source article represented by the pack.
    /// </summary>
    /// <remarks>
    /// The quantity must be greater than zero.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the quantity is less than or equal to zero.
    /// </exception>
    public Scalar Quantity { get; private set; }

    public void SetQuantity(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "The pack quantity must be greater than zero.");
        }

        Quantity = quantity;
    }
}
