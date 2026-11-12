using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using System.Linq.Expressions;

namespace Fargo.Application.Dtos.Partitions;

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool? IsGlobalPartition
);

public static class PartitionDtoMappings
{
    public static readonly Expression<Func<Partition, PartitionDto>> Projection = partition
        => new PartitionDto(
        partition.Guid,
        partition.Name,
        partition.Description,
        partition.ParentPartitionGuid,
        partition.IsGlobalPartition);
}
