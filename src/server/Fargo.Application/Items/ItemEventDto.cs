using Fargo.Core.Items.Events;
using UnitsNet;

namespace Fargo.Application.Items;

/// <summary>
/// Represents a data transfer object for an item event recorded against an
/// event-tracked article.
/// </summary>
/// <param name="Guid">The unique identifier of the item event.</param>
/// <param name="ArticleGuid">
/// The unique identifier of the event-tracked article the event concerns.
/// </param>
/// <param name="EventType">The kind of event recorded.</param>
/// <param name="OccuredAt">The instant, in UTC, at which the event occurred.</param>
/// <param name="Moviment">
/// The movement payload when <paramref name="EventType"/> is
/// <see cref="ItemEventType.Moviment"/>; otherwise <see langword="null"/>.
/// </param>
public sealed record ItemEventDto(
    Guid Guid,
    Guid ArticleGuid,
    ItemEventType EventType,
    DateTimeOffset OccuredAt,
    ItemMovimentEventDto? Moviment
);

/// <summary>
/// Represents the movement payload of an item event.
/// </summary>
/// <param name="Quantity">The strictly positive quantity moved.</param>
/// <param name="FromItemContainerGuid">
/// The origin container, or <see langword="null"/> when the movement originated
/// outside any container, or when the origin container has since been deleted.
/// </param>
/// <param name="FromNoContainer">
/// Whether the movement originally had no origin container.
/// </param>
/// <param name="ToItemContainerGuid">
/// The destination container, or <see langword="null"/> when the movement left
/// the quantity outside any container, or when the destination container has
/// since been deleted.
/// </param>
/// <param name="ToNoContainer">
/// Whether the movement originally had no destination container.
/// </param>
public sealed record ItemMovimentEventDto(
    Scalar Quantity,
    Guid? FromItemContainerGuid,
    bool FromNoContainer,
    Guid? ToItemContainerGuid,
    bool ToNoContainer
);
