using Fargo.Sdk.Contracts.Articles;

namespace Fargo.Sdk.Articles;

/// <summary>Low-level HTTP transport for article endpoints.</summary>
public interface IArticleHttpClient
{
    /// <summary>Retrieves a single article by its unique identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ArticleInfo>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a single article by barcode and barcode type.</summary>
    /// <param name="articleBarcode">The barcode route value.</param>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ArticleInfo>> GetByBarcodeAsync(
        ArticleBarcode articleBarcode,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged, optionally filtered list of articles.</summary>
    /// <param name="temporalAsOf">Optional point-in-time for temporal queries.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">Maximum results per page.</param>
    /// <param name="insideAnyOfThisPartitions">Filters to articles inside any of these partitions.</param>
    /// <param name="notInsideAnyPartition">When <see langword="true"/>, includes articles without a partition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>Creates a new article and returns its assigned identifier.</summary>
    /// <param name="request">The article creation request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        ArticleCreateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces an existing article (full PUT semantics).</summary>
    /// <param name="articleGuid">The unique identifier of the article to update.</param>
    /// <param name="request">The article update request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        ArticleUpdateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an article by its unique identifier.</summary>
    /// <param name="articleGuid">The unique identifier of the article to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}
