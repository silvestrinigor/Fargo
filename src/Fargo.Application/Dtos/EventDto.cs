namespace Fargo.Application.Dtos
{
    public sealed record EventDto(
        Guid Guid,
        DateTime OccurredAt
        );
}
