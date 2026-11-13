using Fargo.Core.Events;

namespace Fargo.Application.Dtos.Items;

public sealed record ItemMovimentDto(
    Guid? MovedToItemContainerGuid,
    DateTimeOffset OccurredAt
);

public static class ItemMovimentDtoMapping
{
    public static ItemMovimentDto ToMovimentDto(this ItemEvent itemEvent)
    {
        return new ItemMovimentDto(itemEvent.ToItemContainerGuid, itemEvent.OccuredAt);
    }
}
