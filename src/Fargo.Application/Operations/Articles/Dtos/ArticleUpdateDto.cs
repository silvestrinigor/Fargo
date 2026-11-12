using Fargo.Core.Informations;
using UnitsNet;

namespace Fargo.Application.Operations.Articles.Dtos;

/// <summary>
/// <see langword="null"/> leaves a value unchanged. The <c>Remove*</c> flags clear the value and take
/// precedence over the matching new value.
/// </summary>
public sealed record ArticleUpdateDto(
    Name? Name = null,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    bool RemoveShelfLife = false,
    Mass? Mass = null,
    bool RemoveMass = false,
    ArticleDimensionUpdateDto? Dimension = null,
    ArticleBarcodeUpdateDto? Barcode = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);
