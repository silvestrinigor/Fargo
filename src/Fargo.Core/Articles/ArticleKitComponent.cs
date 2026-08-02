using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines one component of a kit article.
/// </summary>
public sealed class ArticleKitComponent
{
    public Guid KitArticleGuid { get; private init; }

    public Article KitArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the source article included in the kit.
    /// </summary>
    public Guid FromArticleGuid { get; private set; }

    /// <summary>
    /// Gets the source article included in the kit.
    /// </summary>
    public Article FromArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the quantity of the source article included in the kit.
    /// </summary>
    public Scalar Quantity { get; private set; }

    private ArticleKitComponent()
    {
    }

    public ArticleKitComponent(Article kitArticle, Article fromArticle, Scalar quantity)
    {
        KitArticle = kitArticle;
        KitArticleGuid = kitArticle.Guid;
        FromArticle = fromArticle;
        FromArticleGuid = fromArticle.Guid;
        SetQuantity(quantity);
    }

    public void SetQuantity(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "A kit component quantity must be greater than zero.");
        }

        Quantity = quantity;
    }
}
