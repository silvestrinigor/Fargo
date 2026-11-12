using Fargo.Core.Barcodes;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// The barcodes of an <see cref="Article"/>, one optional property per symbology.
/// </summary>
public class ArticleBarcode
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public Ean13? Ean13 { get; set; }

    // Required by EF Core.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleBarcode()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ArticleBarcode(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        Article = article;
        ArticleGuid = article.Guid;
    }
}
