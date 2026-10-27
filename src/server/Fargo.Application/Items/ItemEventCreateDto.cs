using Fargo.Core.Common;
using Fargo.Core.Items.Events;
using UnitsNet;

namespace Fargo.Application.Items;

/// <summary>
/// Describes a new item event to record against an event-tracked article.
/// </summary>
/// <remarks>
/// The <see cref="EventType"/> acts as a discriminator for the payload.
/// Currently the only supported event type is
/// <see cref="ItemEventType.Moviment"/>, which records a movement of a quantity
/// between containers (or into/out of no container). The movement fields
/// (<see cref="ArticleGuid"/>, <see cref="Quantity"/>,
/// <see cref="FromItemContainerGuid"/>, <see cref="ToItemContainerGuid"/>,
/// <see cref="OccuredAt"/>) describe that movement.
/// </remarks>
/// <param name="EventType">The kind of event to record.</param>
/// <param name="ArticleGuid">
/// The unique identifier of the event-tracked article the event concerns.
/// </param>
/// <param name="Quantity">The strictly positive quantity moved.</param>
/// <param name="FromItemContainerGuid">
/// The origin container, or <see langword="null"/> when the quantity entered
/// stock from outside any container (a receipt).
/// </param>
/// <param name="ToItemContainerGuid">
/// The destination container, or <see langword="null"/> when the quantity left
/// stock to outside any container (a dispatch).
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
    DateTimeOffset? OccuredAt = null
)
{
    /// <summary>
    /// Translates this event-creation request into the movement command payload.
    /// </summary>
    /// <returns>The movement description carried by the create request.</returns>
    /// <exception cref="FargoCoreException">
    /// Thrown when <see cref="EventType"/> is not a supported event type for
    /// creation.
    /// </exception>
    public ItemMovementCreateDto ToMovement()
    {
        if (EventType != ItemEventType.Moviment)
        {
            throw new FargoCoreException(
                $"Unsupported item event type '{EventType}' for creation.",
                FargoErrorType.InvalidOperation);
        }

        return new ItemMovementCreateDto(
            ArticleGuid,
            Quantity,
            FromItemContainerGuid,
            ToItemContainerGuid,
            OccuredAt);
    }
}
