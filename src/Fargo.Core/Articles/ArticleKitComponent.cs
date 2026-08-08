using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Articles;

/// <summary>
/// Defines one component of a kit article.
/// </summary>
/// <remarks>
/// A kit component associates a source <see cref="Article"/> with a quantity,
/// indicating that the source article is included in a kit article.
/// </remarks>
public sealed class ArticleKitComponent
{
    // <summary>
    /// Gets the unique identifier of the kit article.
    /// </summary>
    public Guid KitArticleGuid { get; private init; }

    /// <summary>
    /// Gets the kit article that owns this component.
    /// </summary>
    public Article KitArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the source article included in the kit.
    /// </summary>
    public Guid FromArticleGuid { get; private set; }

    /// <summary>
    /// Gets the article included as a component of the kit.
    /// </summary>
    public Article FromArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the quantity of the source article included in the kit.
    /// </summary>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// Initializes a new <see cref="ArticleKitComponent"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ArticleKitComponent()
    {
    }

    /// <summary>
    /// Initializes a new kit component.
    /// </summary>
    /// <param name="kitArticle">
    /// The kit article.
    /// </param>
    /// <param name="fromArticle">
    /// The article included in the kit.
    /// </param>
    /// <param name="quantity">
    /// The quantity of the article included in the kit.
    /// </param>
    internal ArticleKitComponent(Article kitArticle, Article fromArticle, Scalar quantity)
    {
        if (kitArticle.Guid == fromArticle.Guid)
        {
            throw new FargoCoreException(
                "A kit cannot contain itself.",
                FargoCoreErrorType.InvalidArgument);
        }

        KitArticle = kitArticle;
        KitArticleGuid = kitArticle.Guid;

        FromArticle = fromArticle;
        FromArticleGuid = fromArticle.Guid;

        ValidateQuantity(quantity);

        Quantity = quantity;
    }

    private void ValidateQuantity(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "A kit component quantity must be greater than zero.");
        }
    }
}
