using Fargo.Domain;

namespace Fargo.Application.Partitions;

public sealed record PartitionInformation(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid = null
);
