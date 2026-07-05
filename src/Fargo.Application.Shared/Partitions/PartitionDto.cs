using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Partitions;

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool IsActive
);