using Fargo.Application.Common;
using Fargo.Application.Shared.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed record UserGroupsQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null
) : IQuery<IReadOnlyCollection<UserGroupDto>>;
