using Fargo.Core;
using Fargo.Core.Partitions;
using System.Linq.Expressions;

namespace Fargo.Application.Partitions;

#region DTOs

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record PartitionCreateDto(
    Name Name,
    Description? Description = null,
    Guid? ParentPartitionGuid = null
);

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null
);

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

#endregion DTOs
