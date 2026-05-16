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
    Ean13? Ean13,
    Ean8? Ean8,
    UpcA? UpcA,
    UpcE? UpcE,
    Code128? Code128,
    Code39? Code39,
    Itf14? Itf14,
    Gs1128? Gs1128,
    QrCode? QrCode,
    DataMatrix? DataMatrix,
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

public readonly record struct OptionalValue<TValue>(bool IsSpecified, TValue? Value)
    where TValue : struct
{
    public static OptionalValue<TValue> FromValue(TValue? value) => new(true, value);
}

public readonly record struct OptionalReferenceValue<TValue>(bool IsSpecified, TValue? Value)
    where TValue : class
{
    public static OptionalReferenceValue<TValue> FromValue(TValue? value) => new(true, value);
}

public sealed record ArticleCreateDto(
    Name Name,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
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
    bool? IsActive = null
);

public sealed record ArticlePatchDto(
    Name? Name = default,
    Description? Description = default,
    OptionalValue<TimeSpan> ShelfLife = default,
    ArticleMetricsDto? Metrics = default,
    ArticleBarcodeDto? Barcodes = default,
    IReadOnlyCollection<Guid>? Partitions = default,
    OptionalValue<bool> IsActive = default
);

public sealed record ArticleBarcodeDto(string Barcode, BarcodeFormat Type);

public static class ArticleDtoMappings
{
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
        article.Ean13,
        article.Ean8,
        article.UpcA,
        article.UpcE,
        article.Code128,
        article.Code39,
        article.Itf14,
        article.Gs1128,
        article.QrCode,
        article.DataMatrix,
        article.Partitions.Select(partition => partition.Guid).ToArray(),
        article.IsActive,
        article.EditedByGuid,
        article.ModificationTypes
    );
}
