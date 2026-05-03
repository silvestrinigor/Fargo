using Fargo.Domain;
using Fargo.Domain.Barcodes;
using UnitsNet;

namespace Fargo.Application.Articles;

public sealed record ArticleDto(
    Guid Guid,
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    ArticleMetricsDto? Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record ArticleUpdateDto(
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    ArticleMetricsDto? Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
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

public sealed record ArticleMetricsDto(
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);