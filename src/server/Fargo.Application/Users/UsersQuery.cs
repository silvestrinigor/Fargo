using Fargo.Application.Common;

namespace Fargo.Application.Users;

public sealed record UsersQuery(
    Pagination WithPagination,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null
) : IQuery<IReadOnlyCollection<UserDto>>;
