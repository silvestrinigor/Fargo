using Fargo.Core.Events;
using UnitsNet;

namespace Fargo.Application.Operations.Items.Dtos;

/// <remarks>
/// <see cref="EventType"/> selects which fields apply: a movement uses
/// <see cref="FromItemContainerGuid"/> and <see cref="ToItemContainerGuid"/>; a creation or deletion
/// uses <see cref="ItemContainerGuid"/>. Event-based stock is not unit-tracked, so one event can
/// create or delete many units.
/// </remarks>
/// <param name="FromItemContainerGuid">
/// <see langword="null"/> for a receipt from outside any container.
/// </param>
/// <param name="ToItemContainerGuid">
/// <see langword="null"/> for a dispatch to outside any container.
/// </param>
/// <param name="ItemContainerGuid"><see langword="null"/> when outside any container.</param>
/// <param name="OccuredAt">
/// UTC. <see langword="null"/> makes the server record the current instant.
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
