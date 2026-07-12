using Fargo.Core.Shared;

namespace Fargo.Application.Shared.Partitions;

public sealed record PartitionCreateDto(
    Name Name,
    Description? Description = null,
    Guid? ParentPartitionGuid = null);
