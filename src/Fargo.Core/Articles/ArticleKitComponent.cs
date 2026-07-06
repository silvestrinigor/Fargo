using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines one component of a kit article.
/// </summary>
public sealed class ArticleKitComponent
{
    private ArticleKitComponent()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="article"></param>
    /// <param name="quantity"></param>
    public ArticleKitComponent(Article article, Scalar quantity)
    {
        Article = article;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Gets the unique identifier of the source article included in the kit.
    /// </summary>
    public Guid ArticleGuid { get; private set; }

    /// <summary>
    /// Gets the source article included in the kit.
    /// </summary>
    public Article Article
    {
        get;
        private set
        {
            ArticleGuid = value.Guid;
            field = value;
        }
    } = null!;

    /// <summary>
    /// Gets the quantity of the source article included in the kit.
    /// </summary>
    public Scalar Quantity { get; private set; }

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
