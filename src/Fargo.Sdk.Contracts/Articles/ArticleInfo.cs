namespace Fargo.Sdk.Contracts.Articles;

/// <summary>Represents the article areas changed by the last edit operation.</summary>
[Flags]
public enum ArticleModifiedType
{
    None = 0,
    General = 1 << 0,
    Metrics = 1 << 1,
    Barcode = 1 << 2,
    Partition = 1 << 3,
    Container = 1 << 4,
    Relation = 1 << 5
}

/// <summary>Represents an article returned by the API.</summary>
public sealed record ArticleInfo(
    Guid Guid,
    string Name,
    string Description,
    ArticleMetricsInfo? Metrics,
    TimeSpan? ShelfLife,
    ArticleBarcodesInfo? Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid,
    ArticleModifiedType ModificationTypes);
