namespace Fargo.Core.Entities.Items;

/// <summary>
/// A movement of an item between containers or out of all containers.
/// </summary>
public class ItemMoviment
{
    public Guid ItemGuid { get; private init; }

    /// <summary>
    /// <see langword="null"/> when the movement removed the item from all containers, or when the
    /// destination container was deleted afterwards.
    /// </summary>
    public Guid? MovedToContainerGuid { get; private init; }

    /// <summary>
    /// Whether the movement originally removed the item from all containers. Do not infer it from
    /// <see cref="MovedToContainerGuid"/>, which also becomes <see langword="null"/> when the
    /// destination container is deleted.
    /// </summary>
    public bool RemovedFromContainers { get; private init; } = false;

    public DateTimeOffset OccurredAt { get; private init; }

    private ItemMoviment() { }

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
