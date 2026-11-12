using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.ServiceAccounts.Dtos;

/// <param name="IsActive">Defaults to true.</param>
/// <param name="PartitionsToAdd">
/// Partitions the account belongs to. Distinct from <paramref name="PartitionAccessesToAdd"/>.
/// </param>
public sealed record ServiceAccountCreateDto(
    string ClientId,
    string? Description = null,
    bool IsActive = true,
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null
);
