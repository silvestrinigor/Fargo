namespace Fargo.Application.Items;

public sealed record ItemMovimentDto(
    Guid? MovedToItemContainerGuid,
    DateTimeOffset OccurredAt
);
