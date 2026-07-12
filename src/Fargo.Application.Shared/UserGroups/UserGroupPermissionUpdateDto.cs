using Fargo.Core.Shared;

namespace Fargo.Application.Shared.UserGroups;

public sealed record UserGroupPermissionUpdateDto(
    ActionType Action
);
