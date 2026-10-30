using Fargo.Application.Common;

namespace Fargo.Application.Users;

public sealed record UsersQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    Sort? Sort = null,
    string? Search = null
) : IQuery<PagedResult<UserDto>>;
