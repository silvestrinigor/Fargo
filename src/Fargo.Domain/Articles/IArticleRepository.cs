namespace Fargo.Domain.Articles;

/// <summary>
/// Defines the repository contract for managing <see cref="Article"/> entities.
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// Gets an article by its unique identifier.
    /// </summary>
    Task<Article?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of article unique identifiers.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of article unique identifiers,
    /// filtered to articles that belong to at least one of the specified partitions.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether the specified article has any associated items.
    /// </summary>
    Task<bool> HasItemsAssociated(
        Guid articleGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new article to the persistence context.
    /// </summary>
    void Add(Article article);

    /// <summary>
    /// Removes an article from the persistence context.
    /// </summary>
    void Remove(Article article);
}
