using Fargo.Core.Items.Events;
using UnitsNet;

namespace Fargo.Application.Items;

/// <summary>
/// Describes a new item event to record against an event-tracked article.
/// </summary>
/// <remarks>
/// The <see cref="EventType"/> acts as a discriminator that selects which fields
/// apply:
/// <list type="bullet">
///   <item>
///     <description>
///     <see cref="ItemEventType.Moviment"/> uses
///     <see cref="FromItemContainerGuid"/> and <see cref="ToItemContainerGuid"/>
///     to describe a movement of <see cref="Quantity"/> between containers (or
///     into/out of no container).
///     </description>
///   </item>
///   <item>
///     <description>
///     <see cref="ItemEventType.Creation"/> and
///     <see cref="ItemEventType.Deletion"/> use <see cref="ItemContainerGuid"/>
///     to describe where a <see cref="Quantity"/> of stock was created or
///     deleted. Event-based stock is not unit-tracked, so a single event can
///     create or delete many units at once.
///     </description>
///   </item>
/// </list>
/// </remarks>
/// <param name="EventType">The kind of event to record.</param>
/// <param name="ArticleGuid">
/// The unique identifier of the event-tracked article the event concerns.
/// </param>
/// <param name="Quantity">The strictly positive quantity.</param>
/// <param name="FromItemContainerGuid">
/// For a movement, the origin container, or <see langword="null"/> for a receipt
/// from outside any container.
/// </param>
/// <param name="ToItemContainerGuid">
/// For a movement, the destination container, or <see langword="null"/> for a
/// dispatch to outside any container.
/// </param>
/// <param name="ItemContainerGuid">
/// For a creation or deletion, the container the quantity was created in or
/// deleted from, or <see langword="null"/> when outside any container.
/// </param>
/// <param name="OccuredAt">
/// The instant, in UTC, at which the event occurred. When
/// <see langword="null"/>, the server records the current instant.
/// </param>
public sealed record ItemEventCreateDto(
    ItemEventType EventType,
    Guid ArticleGuid,
    Scalar Quantity,
    Guid? FromItemContainerGuid = null,
    Guid? ToItemContainerGuid = null,
    Guid? ItemContainerGuid = null,
    DateTimeOffset? OccuredAt = null
);
