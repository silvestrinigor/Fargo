using Fargo.Application.Shared.Users;

namespace Fargo.Application.Users;

public sealed record UsersQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<UserDto>>;
