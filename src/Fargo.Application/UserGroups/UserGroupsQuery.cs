using Fargo.Application.Shared.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<UserGroupDto>>;
