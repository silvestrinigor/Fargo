using Fargo.Core.Barcodes;
using Fargo.Core.Common;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// Article rules that need repository access and so cannot be enforced by <see cref="Article"/>.
/// </summary>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    /// <exception cref="FargoCoreException">
    /// The article has associated items or is a dependency of another article.
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

    /// <exception cref="FargoCoreException">
    /// The EAN-13 is already assigned to another article.
    /// </exception>
    public async Task ValidateEan13IsAvailableAsync(Ean13 ean13, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByEan13Async(ean13, cancellationToken);

        if (exists)
        {
            throw new FargoCoreException(
                $"The ean13 '{ean13}' is already assigned to another article.",
                FargoErrorType.Conflict);
        }
    }
}
