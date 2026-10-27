using UnitsNet;

namespace Fargo.Application.Items;

/// <summary>
/// Describes a movement of a quantity of an event-tracked article between
/// containers.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the event-tracked article being moved.
/// </param>
/// <param name="Quantity">
/// The strictly positive quantity moved.
/// </param>
/// <param name="FromItemContainerGuid">
/// The origin container, or <see langword="null"/> when the quantity entered
/// stock from outside any container (a receipt).
/// </param>
/// <param name="ToItemContainerGuid">
/// The destination container, or <see langword="null"/> when the quantity left
/// stock to outside any container (a dispatch).
/// </param>
/// <param name="OccuredAt">
/// The instant, in UTC, at which the movement occurred. When
/// <see langword="null"/>, the server records the current instant. Provide a
/// value to back-date a movement, for example when importing historical data.
/// </param>
/// <remarks>
/// At least one of <paramref name="FromItemContainerGuid"/> or
/// <paramref name="ToItemContainerGuid"/> must be provided, and they must not be
/// equal.
/// </remarks>
public sealed record ItemMovementCreateDto(
    Guid ArticleGuid,
    Scalar Quantity,
    Guid? FromItemContainerGuid = null,
    Guid? ToItemContainerGuid = null,
    DateTimeOffset? OccuredAt = null
);
