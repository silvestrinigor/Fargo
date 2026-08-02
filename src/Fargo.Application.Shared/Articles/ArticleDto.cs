using Fargo.Core.Shared;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleDto(
    Guid Guid,
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    Color? Color,
    Mass? Mass,
    ArticleDimensionDto Dimension,
    ArticleBarcodeDto Barcodes,
    IReadOnlyCollection<Guid> Partitions
);
