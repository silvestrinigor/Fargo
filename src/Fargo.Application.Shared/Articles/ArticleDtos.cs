using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleDto(
    Guid Guid,
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    Color? Color,
    ArticleMetricsDto Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record ArticleMetricsDto(
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);

public sealed record ArticleBarcodesDto(
    Ean13? Ean13 = null,
    Ean8? Ean8 = null,
    UpcA? UpcA = null,
    UpcE? UpcE = null,
    Code128? Code128 = null,
    Code39? Code39 = null,
    Itf14? Itf14 = null,
    Gs1128? Gs1128 = null,
    QrCode? QrCode = null,
    DataMatrix? DataMatrix = null
);

public sealed record ArticleCreateKitPackDto(
    Guid ArticleGuid,
    Scalar Quantity
);

public sealed record ArticleCreateVariationDto(
    Guid FromArticleGuid
);

public sealed record ArticleCreatePackDto(
    Guid FromArticleGuid,
    Scalar Quantity
);

public sealed record ArticleCreateKitDto(
    IReadOnlyCollection<ArticleCreateKitPackDto> Packs
);

public sealed record ArticleCreateContainerDto(
    Mass? MaxMass = null
);

public sealed record ArticleCreateDto(
    Name Name,
    ArticleType ArticleType,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null,
    ArticleCreateVariationDto? Variation = null,
    ArticleCreatePackDto? Pack = null,
    ArticleCreateKitDto? Kit = null,
    ArticleCreateContainerDto? Container = null
);

public sealed record ArticlePatchDto(
    Name? Name = default,
    Description? Description = default,
    TimeSpan? ShelfLife = default,
    bool? RemoveShelfLife = default,
    ArticleMetricsDto? Metrics = default,
    ArticleBarcodesDto? Barcodes = default,
    IReadOnlyCollection<Guid>? Partitions = default,
    bool? IsActive = default
);
