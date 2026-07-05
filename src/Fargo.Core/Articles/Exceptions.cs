using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Exception thrown when an attempt is made to delete an article
/// that still has items associated with it.
/// </summary>
public sealed class ArticleDeleteWithItemsAssociatedFargoDomainException(Guid articleGuid)
    : Exception($"Article '{articleGuid}' cannot be deleted because it has associated items.")
{
    /// <summary>
    /// Gets the identifier of the article that could not be deleted.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}

/// <summary>
/// Exception thrown when a barcode is already assigned to a different article.
/// </summary>
public sealed class ArticleBarcodeAlreadyInUseFargoDomainException(Barcode barcode)
    : Exception($"Barcode '{barcode}' is already assigned to another article.")
{
    public Barcode Barcode { get; } = barcode;
}

/// <summary>
/// Exception thrown when a container-only article operation is requested for a non-container article.
/// </summary>
public sealed class ArticleIsNotContainerFargoDomainException(Guid articleGuid)
    : Exception($"Article '{articleGuid}' is not a container.")
{
    /// <summary>
    /// Gets the identifier of the article that is not a container.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}
