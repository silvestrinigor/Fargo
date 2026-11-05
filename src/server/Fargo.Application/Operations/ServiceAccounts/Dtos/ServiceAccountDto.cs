using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.ServiceAccounts.Dtos;

/// <summary>Read model for a service account. Never exposes the secret.</summary>
public sealed record ServiceAccountDto(
    Guid Guid,
    string ClientId,
    string Description,
    bool IsActive,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> PartitionGuids,
    IReadOnlyCollection<Guid> PartitionAccessGuids
);
