using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;

namespace Fargo.Application.Articles;

public interface IArticleQueryRepository
{
    Task<ArticleInformation?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<ArticleInformation?> GetInfoByGuidInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfoWithNoPartition(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfoInPartitions(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<ArticleInformation?> GetInfoByGuidPublicOrInPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ArticleInformation>> GetManyInfoInPartitionsOrPublic(
        Pagination pagination,
        IReadOnlyCollection<Guid> partitionGuids,
        DateTimeOffset? asOfDateTime = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionInformation>?> GetPartitions(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? partitionFilter = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the first barcode in <paramref name="barcodes"/> that is already assigned
    /// to an article other than <paramref name="excludeArticleGuid"/>,
    /// or <see langword="null"/> if there are no conflicts.
    /// </summary>
    Task<(BarcodeFormat Format, string Code)?> FindConflictingBarcode(
        ArticleBarcodes barcodes,
        Guid excludeArticleGuid,
        CancellationToken cancellationToken = default
    );
}
