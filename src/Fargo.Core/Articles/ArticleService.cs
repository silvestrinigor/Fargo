using Fargo.Core.Common;
using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Provides domain operations and validation rules for <see cref="Article"/> entities.
/// </summary>
/// <remarks>
/// This service contains business rules that require repository access and
/// therefore cannot be enforced by the <see cref="Article"/> entity alone.
/// </remarks>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    /// <summary>
    /// Validates that the specified article can be deleted.
    /// </summary>
    /// <param name="article">
    /// The article to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when the article has associated items or is used as a dependency
    /// by another article.
    /// </exception>
    public async Task ValidateArticleCanBeDeletedAsync(Article article, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(article);

        var hasItems = await articleRepository.HasItemsAssociatedAsync(article.Guid, cancellationToken);

        if (hasItems)
        {
            throw new FargoCoreException($"Article '{article.Guid}' cannot be deleted because it has associated items.");
        }

        var isArticleDependence = await articleRepository.IsDependencyOfAnotherArticleAsync(article.Guid, cancellationToken);

        if (isArticleDependence)
        {
            throw new FargoCoreException($"Article '{article.Guid}' cannot be deleted because it is a dependency of another article.");
        }
    }

    /// <summary>
    /// Validates that the specified EAN-13 barcode is available for assignment
    /// to an article.
    /// </summary>
    /// <param name="ean13">
    /// The EAN-13 barcode to validate.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <exception cref="FargoCoreException">
    /// Thrown when the EAN-13 barcode is already assigned to another article.
    /// </exception>
    public async Task ValidateEan13IsAvailableAsync(Ean13 ean13, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByEan13Async(ean13, cancellationToken);

        if (exists)
        {
            throw new FargoCoreException(
                $"The ean13 '{ean13}' is already assigned to another article.",
                FargoCoreErrorType.InvalidOperation);
        }
    }
}
