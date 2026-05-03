namespace Fargo.Api.Articles;

/// <summary>Low-level HTTP transport for article endpoints.</summary>
public interface IArticleHttpClient
{
    /// <summary>Retrieves a single article by its unique identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ArticleResult>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of articles.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="partitionGuid">Filter to articles in this partition.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="noPartition">When <see langword="true"/>, returns only articles without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new article and returns its assigned identifier.</summary>
    /// <param name="name">The article name.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="partitions">The partitions to assign on creation.</param>
    /// <param name="barcodes">The initial barcodes for the article.</param>
    /// <param name="metrics">Optional physical measurements (mass and dimensions).</param>
    /// <param name="shelfLife">Optional shelf life.</param>
    /// <param name="isActive">Whether the article should be active. Defaults to <see langword="true"/>.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing article (full PUT semantics).</summary>
    /// <param name="articleGuid">The unique identifier of the article to update.</param>
    /// <param name="name">The new name.</param>
    /// <param name="description">The new description.</param>
    /// <param name="partitions">The new full partition set; missing means cleared.</param>
    /// <param name="barcodes">The new full barcode set; missing means cleared.</param>
    /// <param name="metrics">The new metrics.</param>
    /// <param name="shelfLife">The new shelf life.</param>
    /// <param name="isActive">Whether the article is active.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool isActive = true,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an article by its unique identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}
