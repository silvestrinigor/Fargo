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
            notInsideAnyPartition: null,
            cancellationToken
        );

        if (user is not null && user.Partitions.Any(p => !actor.PartitionAccessesGuids.Contains(p)))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
        }

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
            : actor.PartitionAccessesGuids;

        var users = await userRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            query.NotInsideAnyPartition,
            cancellationToken
        );

        if (users.Any(u => u.Partitions.Any(p => !actor.PartitionAccessesGuids.Contains(p))))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
        }

        return users;
    }
}

#endregion Many
