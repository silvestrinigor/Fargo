namespace Fargo.Application.Operations.Items.Dtos;

public sealed record ItemMovimentDto(
    Guid? MovedToItemContainerGuid,
    DateTimeOffset OccurredAt
);
