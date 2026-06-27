using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

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
    Description? Description = null,
    ArticleType? ArticleType = null,
    Guid? FromArticle = null,
    Scalar? PackQuantity = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null,
    Ean13? Ean13 = null,
    Ean8? Ean8 = null,
    UpcA? UpcA = null,
    UpcE? UpcE = null,
    Code128? Code128 = null,
    Code39? Code39 = null,
    Itf14? Itf14 = null,
    Gs1128? Gs1128 = null,
    QrCode? QrCode = null,
    DataMatrix? DataMatrix = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null);

public sealed record ArticleUpdateDto(
    Name? Name = null,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    bool? RemoveShelfLife = null,
    Mass? Mass = null,
    bool? RemoveMass = null,
    Length? LengthX = null,
    bool? RemoveLengthX = null,
    Length? LengthY = null,
    bool? RemoveLengthY = null,
    Length? LengthZ = null,
    bool? RemoveLengthZ = null,
    Ean13? Ean13 = null,
    bool? RemoveEan13 = null,
    Ean8? Ean8 = null,
    bool? RemoveEan8 = null,
    UpcA? UpcA = null,
    bool? RemoveUpcA = null,
    UpcE? UpcE = null,
    bool? RemoveUpcE = null,
    Code128? Code128 = null,
    bool? RemoveCode128 = null,
    Code39? Code39 = null,
    bool? RemoveCode39 = null,
    Itf14? Itf14 = null,
    bool? RemoveItf14 = null,
    Gs1128? Gs1128 = null,
    bool? RemoveGs1128 = null,
    QrCode? QrCode = null,
    bool? RemoveQrCode = null,
    DataMatrix? DataMatrix = null,
    bool? RemoveDataMatrix = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    bool? IsActive = null);
