using Fargo.Application.Shared.Partitions;
using Fargo.Core.Partitions;
using System.Linq.Expressions;

namespace Fargo.Application.Partitions;

public static class PartitionDtoMappings
{
    public static readonly Expression<Func<Partition, PartitionDto>> Projection = partition => new PartitionDto(
        partition.Guid,
        partition.Name,
        partition.Description,
        partition.ParentPartitionGuid,
        partition.IsActive,
        partition.EditedByGuid);
}

