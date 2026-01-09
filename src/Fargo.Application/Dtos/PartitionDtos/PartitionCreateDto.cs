using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.PartitionDtos
{
    public sealed record PartitionCreateDto(
        Name Name,
        Description? Description = null
        );
}
