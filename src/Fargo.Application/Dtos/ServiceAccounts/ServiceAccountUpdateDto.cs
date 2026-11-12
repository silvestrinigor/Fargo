using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Dtos.ServiceAccounts;

public sealed record ServiceAccountUpdateDto(
    string? Description = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);
