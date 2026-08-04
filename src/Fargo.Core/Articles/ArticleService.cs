using Fargo.Core.Shared.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Provides domain operations and validation rules for <see cref="Article"/> entities.
/// </summary>
/// <remarks>
/// This service contains business rules that require repository access and
/// therefore cannot be enforced by the <see cref="Article"/> aggregate alone.
/// </remarks>
public sealed class ArticleService(IArticleRepository articleRepository)
{
    public async Task ValidateArticleCanBeDeletedAsync(Article article, CancellationToken cancellationToken = default)
    {
        var hasItems = await articleRepository.HasItemsAssociatedAsync(article.Guid, cancellationToken);

        if (hasItems)
        {
            throw new FargoCoreException($"Article '{article.Guid}' cannot be deleted because it has associated items.");
        }

        var isArticleDependence = await articleRepository.IsDependenceOfAnotherArticleAsync(article.Guid, cancellationToken);

        if (isArticleDependence)
        {
            throw new FargoCoreException($"Article '{article.Guid}' cannot be deleted because it is a dependency of another article.");
        }
    }

    public async Task ValidateBarcodeIsAvailableAsync(Barcode barcode, CancellationToken cancellationToken = default)
    {
        var exists = await articleRepository.ExistsByBarcodeAsync(barcode, cancellationToken);

        if (exists)
        {
            ThrowBarcodeAlreadyExists();
        }
    }

    private static void ThrowBarcodeAlreadyExists()
    {
        throw new FargoCoreException(
            "The barcode is already assigned to another article.",
            FargoCoreErrorType.InvalidOperation);
    }
}
