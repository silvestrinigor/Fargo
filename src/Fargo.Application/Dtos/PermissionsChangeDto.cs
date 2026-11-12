using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Dtos;

/// <summary>
/// Request body of the dedicated permission endpoints: the permissions to grant and to revoke in
/// one request. At least one must be given, and no permission may appear in both.
/// </summary>
public sealed record PermissionsChangeDto(
    IReadOnlyCollection<ActionType>? PermissionsToAdd = null,
    IReadOnlyCollection<ActionType>? PermissionsToRemove = null
);
