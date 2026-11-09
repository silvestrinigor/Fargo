using Fargo.Core.Entities.Items;

namespace Fargo.Application.Operations.Items.Dtos;

public sealed record ItemMovimentDto(
    Guid? MovedToItemContainerGuid,
    DateTimeOffset OccurredAt
);

public static class ItemMovimentDtoMapping
{
    public static ItemMovimentDto ToDto(this ItemMoviment itemMoviment)
    {
        return new ItemMovimentDto(itemMoviment.MovedToContainerGuid, itemMoviment.OccurredAt);
    }
}
