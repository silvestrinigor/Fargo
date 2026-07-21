using Fargo.Core.Shared;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupCreateDto(
    string Nameid,
    Description? Description = null,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions = null,
    IReadOnlyCollection<Guid>? PartitionsToAdd = null
);
