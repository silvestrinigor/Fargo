using Fargo.Domain.Barcodes;

namespace Fargo.Domain.Articles;

/// <summary>
/// Exception thrown when an attempt is made to delete an article
/// that still has items associated with it.
/// </summary>
public sealed class ArticleDeleteWithItemsAssociatedFargoDomainException(Guid articleGuid)
    : FargoDomainException($"Article '{articleGuid}' cannot be deleted because it has associated items.")
{
    /// <summary>
    /// Gets the identifier of the article that could not be deleted.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}

/// <summary>
/// Exception thrown when a barcode is already assigned to a different article.
/// </summary>
public class ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat format, string code)
    : FargoDomainException($"Barcode '{code}' ({format}) is already assigned to another article.")
{
    /// <summary>Gets the barcode format that conflicts.</summary>
    public BarcodeFormat Format { get; } = format;

    /// <summary>Gets the barcode code that conflicts.</summary>
    public string Code { get; } = code;
}
