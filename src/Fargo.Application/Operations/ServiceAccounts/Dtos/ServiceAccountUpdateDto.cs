using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.ServiceAccounts.Dtos;

/// <summary>Mutable service-account state to apply in an update.</summary>
public sealed record ServiceAccountUpdateDto(
    string? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<ActionType>? PermissionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);
