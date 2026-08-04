using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Represents the collection of barcodes associated with an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// An article may have multiple barcode formats assigned simultaneously.
/// Each barcode type is optional and uniquely identifies the same article
/// using a different standard.
/// </remarks>
public class ArticleBarcode
{
    /// <summary>
    /// Gets the unique identifier of the associated article.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the article associated with these barcodes.
    /// </summary>
    public Article Article { get; private init; }

    /// <summary>
    /// EAN-13 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean13? Ean13 { get; set; }

    /// <summary>
    /// Initializes a new <see cref="ArticleBarcode"/> instance.
    /// </summary>
    /// <remarks>
    /// Required by Entity Framework.
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticleBarcode()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new barcode collection for the specified article.
    /// </summary>
    /// <param name="article">
    /// The article associated with the barcode collection.
    /// </param>
    internal ArticleBarcode(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
    }
}
