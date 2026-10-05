using Fargo.Application.Common;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid
) : IQuery<UserGroupDto?>;
