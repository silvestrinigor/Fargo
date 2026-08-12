using Fargo.Core.Entities;

namespace Fargo.Core.Items;

public class ItemMoviment : IEntity
{
    public Guid Guid { get; private init; }

    public Guid ItemGuid { get; private init; }

    public Guid? MovedToContainerGuid { get; private init; }

    public bool RemovedFromContainers { get; private init; } = false;

    public DateTimeOffset OccurredAt { get; private init; }

    private ItemMoviment() { }

    private ItemMoviment(Guid itemGuid, Guid? parentItemContainerGuid, DateTimeOffset occurredAt)
    {
        ItemGuid = itemGuid;
        MovedToContainerGuid = parentItemContainerGuid;
        OccurredAt = occurredAt;

        if (parentItemContainerGuid is null)
        {
            RemovedFromContainers = true;
        }
    }

    public static ItemMoviment CreateItemMoviment(Guid itemGuid, Guid? parentItemContainerGuid, DateTimeOffset occurredAt)
    {
        return new ItemMoviment(itemGuid, parentItemContainerGuid, occurredAt);
    }
}
