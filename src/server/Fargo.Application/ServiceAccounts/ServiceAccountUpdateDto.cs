using Fargo.Core.Actors;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Mutable service-account state to apply in an update.</summary>
public sealed record ServiceAccountUpdateDto(
    string? Description = null,
    bool? IsActive = null,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<ActionType>? PermissionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);
