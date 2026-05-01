using Fargo.Api.Contracts.Permissions;

namespace Fargo.Api.Contracts.UserGroups;

/// <summary>Represents the user-group payload inside a user-group create request.</summary>
public sealed record UserGroupCreateDto(
    string Nameid,
    string? Description = null,
    IReadOnlyCollection<PermissionUpdateDto>? Permissions = null,
    Guid? FirstPartition = null);
