using Fargo.Application.Common;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<UserGroupDto>>;
