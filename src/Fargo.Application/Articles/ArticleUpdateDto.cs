using Fargo.Application.Articles;
using Fargo.Core.Informations;
using UnitsNet;

namespace Fargo.Application.Shared.Articles;

public sealed record ArticleUpdateDto(
    Name? Name = null,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    bool? RemoveShelfLife = null,
    Mass? Mass = null,
    bool? RemoveMass = null,
    ArticleDimensionUpdateDto? Dimension = null,
    ArticleBarcodeUpdateDto? Barcode = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);
