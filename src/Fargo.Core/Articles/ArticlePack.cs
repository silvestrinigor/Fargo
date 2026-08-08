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
    /// <summary>
    /// Gets the unique identifier of the pack article.
    /// </summary>
    public Guid PackArticleGuid { get; private init; }

    /// <summary>
    /// Gets the pack article.
    /// </summary>
    public Article PackArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the unique identifier of the source article contained in the pack.
    /// </summary>
    public Guid FromArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article from which this pack is composed.
    /// </summary>
    public Article FromArticle { get; private init; } = null!;

    /// <summary>
    /// Gets the quantity of the source article represented by the pack.
    /// </summary>
    public Scalar Quantity { get; private init; }

    /// <summary>
    /// Initializes a new <see cref="ArticlePack"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
    private ArticlePack()
    {
    }

    /// <summary>
    /// Initializes a new pack relationship.
    /// </summary>
    /// <param name="packArticle">
    /// The pack article.
    /// </param>
    /// <param name="fromArticle">
    /// The article represented by the pack.
    /// </param>
    /// <param name="quantity">
    /// The quantity of the source article represented by the pack.
    /// </param>
    internal ArticlePack(Article packArticle, Article fromArticle, Scalar quantity)
    {
        if (packArticle.Guid == fromArticle.Guid)
        {
            throw new FargoCoreException(
                "A pack cannot reference itself.",
                FargoCoreErrorType.InvalidArgument);
        }

        PackArticle = packArticle;
        PackArticleGuid = packArticle.Guid;

        FromArticle = fromArticle;
        FromArticleGuid = fromArticle.Guid;

        ValidateQuantity(quantity);
        Quantity = quantity;
    }

    internal void ValidateQuantity(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "The pack quantity must be greater than zero.");
        }
    }
}
