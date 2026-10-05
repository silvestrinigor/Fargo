using Fargo.Application.Common;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null
) : IQuery<IReadOnlyCollection<UserGroupDto>>;
