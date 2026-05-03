namespace Fargo.Api.Articles;

/// <summary>
/// Provides operations for managing articles.
/// </summary>
public interface IArticleClient
{
    /// <summary>
    /// Gets a single article by its unique identifier.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="temporalAsOf">
    /// When provided, returns the state of the article as it existed at this point in time.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<ArticleResult>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of articles accessible to the current user.
    /// </summary>
    Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new article.
    /// </summary>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an existing article (full PUT semantics).
    /// </summary>
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

    /// <summary>
    /// Deletes an article. The article must have no associated items.
    /// </summary>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}
