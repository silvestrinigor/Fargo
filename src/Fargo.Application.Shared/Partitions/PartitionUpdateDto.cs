using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Partitions;

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null
);
