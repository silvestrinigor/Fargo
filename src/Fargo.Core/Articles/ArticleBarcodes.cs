using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Represents the barcode information associated with an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// An article may have one or more barcodes, with each property representing
/// a different barcode symbology. Each barcode is optional.
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
    /// Gets or sets the EAN-13 barcode associated with the article.
    /// </summary>
    /// <value>
    /// The article's EAN-13 barcode, or <see langword="null"/> when no
    /// EAN-13 barcode is assigned.
    /// </value>
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
    /// Initializes a new instance of the <see cref="ArticleBarcode"/> class
    /// for the specified article.
    /// </summary>
    /// <param name="article">
    /// The article to which the barcode information belongs.
    /// </param>
    internal ArticleBarcode(Article article)
    {
        Article = article;
        ArticleGuid = article.Guid;
    }
}
