namespace Fargo.Core.Items;

/// <summary>
/// Represents a movement of an item between containers or out of all containers.
/// </summary>
/// <remarks>
/// A movement records the container to which the item was moved at the time of
/// the movement. <see cref="MovedToContainerGuid"/> may later become
/// <see langword="null"/> when the referenced container is deleted.
///
/// <see cref="RemovedFromContainers"/> preserves whether the movement originally
/// represented removing the item from all containers, allowing the movement
/// history to distinguish an explicit removal from a movement whose destination
/// container was subsequently deleted.
/// </remarks>
public class ItemMoviment
{
    /// <summary>
    /// Gets the unique identifier of the item that was moved.
    /// </summary>
    public Guid ItemGuid { get; private init; }

    /// <summary>
    /// Gets the identifier of the container to which the item was moved.
    /// </summary>
    /// <remarks>
    /// This value is <see langword="null"/> when the movement removed the item
    /// from all containers. It may also become <see langword="null"/> later if
    /// the destination container is deleted.
    /// </remarks>
    public Guid? MovedToContainerGuid { get; private init; }

    /// <summary>
    /// Gets a value indicating whether this movement originally removed the
    /// item from all containers.
    /// </summary>
    /// <remarks>
    /// This property must not be inferred from <see cref="MovedToContainerGuid"/>
    /// because the destination container may subsequently be deleted, causing
    /// its identifier to become <see langword="null"/>.
    /// </remarks>
    public bool RemovedFromContainers { get; private init; } = false;

    /// <summary>
    /// Gets the date and time at which the movement occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private init; }

    private ItemMoviment() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemMoviment"/> class.
    /// </summary>
    /// <param name="itemGuid">The identifier of the moved item.</param>
    /// <param name="destinationItemContainerGuid">
    /// The identifier of the destination container, or <see langword="null"/>
    /// when the item was removed from all containers.
    /// </param>
    /// <param name="occurredAt">The date and time at which the movement occurred.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemGuid"/> is <see cref="Guid.Empty"/>.
    /// </exception>
    internal ItemMoviment(Guid itemGuid, Guid? destinationItemContainerGuid, DateTimeOffset occurredAt)
    {
        if (itemGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "The item identifier cannot be empty.",
                nameof(itemGuid));
        }

        ItemGuid = itemGuid;
        MovedToContainerGuid = destinationItemContainerGuid;
        OccurredAt = occurredAt;

        if (destinationItemContainerGuid is null)
        {
            RemovedFromContainers = true;
        }
    }
}
