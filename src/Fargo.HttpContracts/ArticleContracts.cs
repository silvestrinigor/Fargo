using System.Text.Json.Serialization;

namespace Fargo.HttpContracts;

public sealed record ArticleDto(
    Guid Guid,
    string Name,
    string Description,
    TimeSpan? ShelfLife,
    string? Color,
    ArticleMetricsDto Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record ArticleMetricsDto(
    UnitValueDto? Mass = null,
    UnitValueDto? LengthX = null,
    UnitValueDto? LengthY = null,
    UnitValueDto? LengthZ = null
);

public sealed record ArticleBarcodesDto(
    string? Ean13 = null,
    string? Ean8 = null,
    string? UpcA = null,
    string? UpcE = null,
    string? Code128 = null,
    string? Code39 = null,
    string? Itf14 = null,
    string? Gs1128 = null,
    string? QrCode = null,
    string? DataMatrix = null
);

public sealed record ArticleCreateKitPackRequest(
    Guid ArticleGuid,
    double Quantity
);

public sealed record ArticleCreateVariationRequest(
    Guid FromArticleGuid
);

public sealed record ArticleCreatePackRequest(
    Guid FromArticleGuid,
    double Quantity
);

public sealed record ArticleCreateKitRequest(
    IReadOnlyCollection<ArticleCreateKitPackRequest> Packs
);

public sealed record ArticleCreateContainerRequest(
    UnitValueDto? MaxMass = null
);

public sealed record ArticleCreateRequest(
    string Name,
    ArticleType ArticleType,
    string? Description = null,
    TimeSpan? ShelfLife = null,
    string? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null,
    ArticleCreateVariationRequest? Variation = null,
    ArticleCreatePackRequest? Pack = null,
    ArticleCreateKitRequest? Kit = null,
    ArticleCreateContainerRequest? Container = null
);

public sealed record ArticlePatchRequest
{
    public ArticlePatchRequest()
    {
    }

    public ArticlePatchRequest(
        string? Name = null,
        string? Description = null,
        bool? RemoveShelfLife = null,
        ArticleMetricsDto? Metrics = null,
        ArticleBarcodesDto? Barcodes = null,
        IReadOnlyCollection<Guid>? Partitions = null,
        bool? IsActive = null)
    {
        this.Name = Name;
        this.Description = Description;
        this.RemoveShelfLife = RemoveShelfLife;
        this.Metrics = Metrics;
        this.Barcodes = Barcodes;
        this.Partitions = Partitions;
        this.IsActive = IsActive;
    }

    public ArticlePatchRequest(
        TimeSpan? ShelfLife,
        string? Name = null,
        string? Description = null,
        ArticleMetricsDto? Metrics = null,
        ArticleBarcodesDto? Barcodes = null,
        IReadOnlyCollection<Guid>? Partitions = null,
        bool? IsActive = null)
        : this(Name, Description, null, Metrics, Barcodes, Partitions, IsActive)
    {
        this.ShelfLife = ShelfLife;
    }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public TimeSpan? ShelfLife { get; init; } = default;

    public bool? RemoveShelfLife { get; init; } = default;

    public ArticleMetricsDto? Metrics { get; init; }

    public ArticleBarcodesDto? Barcodes { get; init; }

    public IReadOnlyCollection<Guid>? Partitions { get; init; }

    public bool? IsActive { get; init; }
}
