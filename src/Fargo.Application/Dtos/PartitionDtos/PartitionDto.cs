using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Dtos.PartitionDtos
{
    public sealed record PartitionDto(
        Guid Guid,
        Name Name,
        Description Description
        );
}
