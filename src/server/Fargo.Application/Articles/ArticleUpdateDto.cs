using Fargo.Core.Informations;
using UnitsNet;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents the changes to apply to an existing article.
/// </summary>
/// <param name="Name">
/// The new name of the article. A <see langword="null"/> value leaves the existing name unchanged.
/// </param>
/// <param name="Description">
/// The new description of the article. A <see langword="null"/> value leaves the existing description unchanged.
/// </param>
/// <param name="ShelfLife">
/// The new shelf life of the article. A <see langword="null"/> value leaves the existing value unchanged.
/// </param>
/// <param name="RemoveShelfLife">
/// Indicates whether the existing shelf life should be removed by setting it to <see langword="null"/>.
/// When <see langword="true"/>, this takes precedence over <paramref name="ShelfLife"/>.
/// </param>
/// <param name="Mass">
/// The new mass of the article. A <see langword="null"/> value leaves the existing value unchanged.
/// </param>
/// <param name="RemoveMass">
/// Indicates whether the existing mass should be removed by setting it to <see langword="null"/>.
/// When <see langword="true"/>, this takes precedence over <paramref name="Mass"/>.
/// </param>
/// <param name="Dimension">
/// The optional dimensional changes to apply to the article.
/// </param>
/// <param name="Barcode">
/// The optional barcode changes to apply to the article.
/// </param>
/// <param name="PartitionsToAdd">
/// The identifiers of partitions to associate with the article.
/// </param>
/// <param name="PartitionsToRemove">
/// The identifiers of partitions to disassociate from the article.
/// </param>
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
