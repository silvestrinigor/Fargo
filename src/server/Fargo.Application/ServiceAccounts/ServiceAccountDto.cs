using Fargo.Core.Actors;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Read model for a service account. Never exposes the secret.</summary>
public sealed record ServiceAccountDto(
    Guid Guid,
    string ClientId,
    string Description,
    bool IsActive,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> PartitionAccessGuids
);
