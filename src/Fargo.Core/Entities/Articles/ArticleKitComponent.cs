using Fargo.Core.Common;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// A quantity of a source article included in a kit article.
/// </summary>
public sealed class ArticleKitComponent
{
    public Guid KitArticleGuid { get; private init; }

    public Article KitArticle { get; private init; } = null!;

    public Guid FromArticleGuid { get; private set; }

    public Article FromArticle { get; private init; } = null!;

    public Scalar Quantity { get; private init; }

    // Required by EF Core.
    private ArticleKitComponent()
    {
    }

    internal ArticleKitComponent(Article kitArticle, Article fromArticle, Scalar quantity)
    {
        ArgumentNullException.ThrowIfNull(kitArticle);
        ArgumentNullException.ThrowIfNull(fromArticle);

        if (kitArticle.Guid == fromArticle.Guid)
        {
            throw new FargoCoreException(
                "A kit cannot contain itself.",
                FargoErrorType.InvalidOperation);
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
