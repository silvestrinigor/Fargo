using Fargo.Application.Authentication;
using Fargo.Domain;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

#region Single

public sealed record UserSingleQuery(
    Guid UserGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserDto?>;

public sealed class UserSingleQueryHandler(
    ActorService actorService,
    IUserQueryRepository userRepository,
    ICurrentUser currentUser,
    ILogger<UserSingleQueryHandler> logger
) : IQueryHandler<UserSingleQuery, UserDto?>
{
    public async Task<UserDto?> Handle(
        UserSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug(
            "User single query started for user {UserGuid} by actor {ActorGuid}.",
            query.UserGuid,
            currentUser.UserGuid);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var user = await userRepository.GetInfoByGuid(
            query.UserGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: true,
            cancellationToken
        );

        logger.LogDebug(
            "User single query completed for user {UserGuid} by actor {ActorGuid}. Found: {Found}.",
            query.UserGuid,
            actor.Guid,
            user is not null);

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
    ICurrentUser currentUser,
    ILogger<UsersQueryHandler> logger
) : IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(
        UsersQuery query,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogDebug(
            "Users query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
            currentUser.UserGuid,
            query.WithPagination.Page,
            query.WithPagination.Limit);

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

        logger.LogDebug(
            "Users query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
            actor.Guid,
            query.InsideAnyOfThisPartitions?.Count ?? 0,
            insideAnyOfThisPartitions?.Count ?? 0,
            users.Count);

        return users;
    }
}

#endregion Many
