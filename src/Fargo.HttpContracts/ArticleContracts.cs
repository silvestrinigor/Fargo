using System.Text.Json.Serialization;

namespace Fargo.HttpContracts;

public sealed record ArticleDto(
    Guid Guid,
    string Name,
    ArticleType ArticleType,
    string Description,
    TimeSpan? ShelfLife,
    string? Color,
    ArticleMetricsDto Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid,
    ArticleModifiedType ModificationTypes
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

public sealed record ArticlePatchRequest(
    string? Name = null,
    string? Description = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    OptionalField<TimeSpan> ShelfLife = default,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
);
