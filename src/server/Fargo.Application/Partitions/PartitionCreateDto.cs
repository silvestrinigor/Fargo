using Fargo.Core.Informations;

namespace Fargo.Application.Partitions;

public sealed record PartitionCreateDto(
    Name Name,
    Guid ParentPartitionGuid,
    Description? Description = null);
