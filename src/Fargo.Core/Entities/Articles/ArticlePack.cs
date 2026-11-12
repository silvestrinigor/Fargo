using Fargo.Core.Common;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// Defines a pack article as a quantity of another article sold or stored as one article,
/// for example twelve units of the source article.
/// </summary>
public sealed class ArticlePack
{
    public Guid PackArticleGuid { get; private init; }

    public Article PackArticle { get; private init; } = null!;

    public Guid FromArticleGuid { get; private init; }

    public Article FromArticle { get; private init; } = null!;

    public Scalar Quantity { get; private init; }

    // Required by EF Core.
    private ArticlePack()
    {
    }

    internal ArticlePack(Article packArticle, Article fromArticle, Scalar quantity)
    {
        ArgumentNullException.ThrowIfNull(packArticle);
        ArgumentNullException.ThrowIfNull(fromArticle);

        if (packArticle.Guid == fromArticle.Guid)
        {
            throw new FargoCoreException(
                "A pack cannot reference itself.",
                FargoErrorType.InvalidOperation);
        }

        PackArticle = packArticle;
        PackArticleGuid = packArticle.Guid;

        FromArticle = fromArticle;
        FromArticleGuid = fromArticle.Guid;

        ValidateQuantity(quantity);
        Quantity = quantity;
    }

    internal static void ValidateQuantity(Scalar quantity)
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
