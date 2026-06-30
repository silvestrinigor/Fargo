using Fargo.Application.Shared.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserGroupDto?>;