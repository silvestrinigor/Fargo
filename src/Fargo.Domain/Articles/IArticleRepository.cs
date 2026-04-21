using Fargo.Domain.Partitions;

namespace Fargo.Domain.Articles;

/// <summary>
/// Defines the repository contract for managing <see cref="Article"/> entities.
/// </summary>
/// <remarks>
/// This repository provides persistence access and domain queries related to
/// <see cref="Article"/> aggregates. It exposes both aggregate retrieval methods
/// and lightweight projection queries used for read operations.
/// </remarks>
public interface IArticleRepository
{
    /// <summary>
    /// Gets an article by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The matching <see cref="Article"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Article?> GetByGuid(
        Guid entityGuid,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets lightweight information about an article by its unique identifier.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the article.</param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned information represents the state of the article
    /// as it existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// An <see cref="ArticleInformation"/> projection if the article exists;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    Task<ArticleInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of article information projections.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the articles
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of <see cref="ArticleInformation"/> objects.
    /// </returns>
    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets lightweight information about an article by its unique identifier,
    /// only if the article belongs to at least one of the specified partitions.
    /// </summary>
    Task<ArticleInformation?> GetInfoByGuidInPartitions(
            Guid entityGuid,
            IReadOnlyCollection<Guid> partitionGuids,
            DateTimeOffset? asOfDateTime = null,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Gets a paginated collection of article information projections
    /// for articles that do not belong to any partition.
    /// </summary>
    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfoWithNoPartition(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of article information projections,
    /// filtered to articles that belong to at least one of the specified partitions.
    /// </summary>
    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfoInPartitions(
            Pagination pagination,
            IReadOnlyCollection<Guid> partitionGuids,
            DateTimeOffset? asOfDateTime = null,
            string? search = null,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Gets a paginated collection of article unique identifiers.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the articles
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of article unique identifiers.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetManyGuids(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paginated collection of article unique identifiers,
    /// filtered to articles that belong to at least one of the specified partitions.
    /// </summary>
    /// <param name="pagination">
    /// The pagination configuration used to control the number of returned results
    /// and the starting position of the query.
    /// </param>
    /// <param name="partitionGuids">
    /// The partitions used to filter accessible articles.
    /// Only articles belonging to at least one of these partitions are returned.
    /// </param>
    /// <param name="asOfDateTime">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the returned results represent the state of the articles
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of article unique identifiers.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetManyGuidsInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the partitions that directly contain the specified article.
    /// </summary>
    /// <param name="entityGuid">The unique identifier of the article.</param>
    /// <param name="partitionFilter">
    /// When provided, only partitions within this set are returned.
    /// Pass <see langword="null"/> to return all partitions (admin/system path).
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A read-only collection of <see cref="PartitionInformation"/> representing the
    /// partitions that directly contain the article; or <see langword="null"/> if
    /// the article does not exist.
    /// </returns>
    Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether the specified article has any associated items.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// <see langword="true"/> if the article has associated items; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> HasItemsAssociated(
            Guid articleGuid,
            CancellationToken cancellationToken = default
            );

    /// <summary>
    /// Adds a new article to the persistence context.
    /// </summary>
    /// <param name="article">The article to add.</param>
    void Add(Article article);

    /// <summary>
    /// Removes an article from the persistence context.
    /// </summary>
    /// <param name="article">The article to remove.</param>
    void Remove(Article article);
}
