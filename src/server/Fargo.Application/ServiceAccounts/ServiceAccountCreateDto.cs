using Fargo.Core.Actors;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Data required to create a service account.</summary>
/// <param name="ClientId">The unique client identifier.</param>
/// <param name="Description">An optional human-readable description.</param>
/// <param name="IsActive">Whether the account is active on creation. Defaults to true.</param>
/// <param name="PermissionsToAdd">Permissions to grant on creation.</param>
/// <param name="PartitionAccessesToAdd">Partition accesses to grant on creation.</param>
public sealed record ServiceAccountCreateDto(
    string ClientId,
    string? Description = null,
    bool IsActive = true,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null
);