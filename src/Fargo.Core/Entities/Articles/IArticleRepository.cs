using Fargo.Core.Barcodes;

namespace Fargo.Core.Entities.Articles;

public interface IArticleRepository
{
    Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEan13Async(Ean13 ean13, CancellationToken cancellationToken = default);

    Task<bool> HasItemsAssociatedAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether another article is a variation or a pack of this article, or a kit that contains it.
    /// </summary>
    Task<bool> IsDependencyOfAnotherArticleAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    void Add(Article article);

    void Remove(Article article);
}
