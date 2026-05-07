using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Users;

#region Single

public sealed record UserSingleQuery(
    Guid UserGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserDto?>;

public sealed class UserSingleQueryHandler(
    ActorService actorService,
    IUserQueryRepository userRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserSingleQuery, UserDto?>
{
    public async Task<UserDto?> Handle(
        UserSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var user = await userRepository.GetInfoByGuid(
            query.UserGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: true,
            cancellationToken
        );

        return user;
    }
}

#endregion Single

#region Many

public sealed record UsersQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? InsideAnyOfThisPartitions = null,
    bool? NotInsideAnyPartition = null
) : IQuery<IReadOnlyCollection<UserDto>>;

public sealed class UsersQueryHandler(
    ActorService actorService,
    IUserQueryRepository userRepository,
    ICurrentUser currentUser
) : IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(
        UsersQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : query.NotInsideAnyPartition is true
                ? null
                : actor.PartitionAccessesGuids;
        var notInsideAnyPartition = query.NotInsideAnyPartition ??
            (query.InsideAnyOfThisPartitions is null ? true : null);

        var users = await userRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            notInsideAnyPartition,
            cancellationToken
        );

        return users;
    }
}

#endregion Many
