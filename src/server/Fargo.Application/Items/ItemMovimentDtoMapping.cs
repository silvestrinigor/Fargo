using Fargo.Core.Items;

namespace Fargo.Application.Items;

public static class ItemMovimentDtoMapping
{
    public static ItemMovimentDto ToDto(this ItemMoviment itemMoviment)
    {
        return new ItemMovimentDto(itemMoviment.MovedToContainerGuid, itemMoviment.OccurredAt);
    }
}
