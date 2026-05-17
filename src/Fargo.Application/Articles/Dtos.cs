using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using System.Drawing;
using System.Linq.Expressions;
using UnitsNet;

namespace Fargo.Application.Articles;

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
    Guid? EditedByGuid,
    ArticleModifiedType ModificationTypes
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

public static class ArticleCommandDtoMappings
{
    public static ArticleMetrics ToCore(this ArticleMetricsDto metrics)
        => new(
            metrics.Mass,
            metrics.LengthX,
            metrics.LengthY,
            metrics.LengthZ);

    public static ArticleBarcodesSet ToCore(this ArticleBarcodesDto barcodes)
        => new(
            barcodes.Ean13,
            barcodes.Ean8,
            barcodes.UpcA,
            barcodes.UpcE,
            barcodes.Code128,
            barcodes.Code39,
            barcodes.Itf14,
            barcodes.Gs1128,
            barcodes.QrCode,
            barcodes.DataMatrix);
}

public sealed record ArticleCreateDto(
    Name Name,
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
    OptionalValue<TimeSpan> ShelfLife = default,
    ArticleMetricsDto? Metrics = default,
    ArticleBarcodesDto? Barcodes = default,
    IReadOnlyCollection<Guid>? Partitions = default,
    bool? IsActive = default
);

/// <summary>
/// Provides mappings for article DTO projections.
/// </summary>
public static class ArticleDtoMappings
{
    /// <summary>
    /// Expression used to project an article entity into an <see cref="ArticleDto"/>.
    /// </summary>
    public static readonly Expression<Func<Article, ArticleDto>> Projection = article => new ArticleDto(
        article.Guid,
        article.Name,
        article.Description,
        article.ShelfLife,
        article.Color,
        new ArticleMetricsDto(
            article.Mass,
            article.LengthX,
            article.LengthY,
            article.LengthZ),
        new ArticleBarcodesDto(
            article.Ean13,
            article.Ean8,
            article.UpcA,
            article.UpcE,
            article.Code128,
            article.Code39,
            article.Itf14,
            article.Gs1128,
            article.QrCode,
            article.DataMatrix),
        article.Partitions.Select(partition => partition.Guid).ToArray(),
        article.IsActive,
        article.EditedByGuid,
        article.ModificationTypes
    );
}
