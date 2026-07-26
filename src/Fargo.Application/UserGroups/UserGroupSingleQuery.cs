using Fargo.Application.Shared.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid
) : IQuery<UserGroupDto?>;
