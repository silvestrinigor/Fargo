using Fargo.Application.Authentication;
using Fargo.Domain;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

#region Single

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserGroupDto?>;

public sealed class UserGroupSingleQueryHandler(
    ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentUser currentUser,
    ILogger<UserGroupSingleQueryHandler> logger
) : IQueryHandler<UserGroupSingleQuery, UserGroupDto?>
{
    public async Task<UserGroupDto?> Handle(
        UserGroupSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User group single query started for user group {UserGroupGuid} by actor {ActorGuid}.",
                query.UserGroupGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var userGroup = await userGroupRepository.GetInfoByGuid(
            query.UserGroupGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User group single query completed for user group {UserGroupGuid} by actor {ActorGuid}. Found: {Found}.",
                query.UserGroupGuid,
                actor.Guid,
                userGroup is not null);
        }

        return userGroup;
    }
}

#endregion Single

#region Many

public sealed record UserGroupsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? InsideAnyOfThisPartitions = null,
    bool? NotInsideAnyPartition = null
) : IQuery<IReadOnlyCollection<UserGroupDto>>;

public sealed class UserGroupsQueryHandler(
    ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentUser currentUser,
    ILogger<UserGroupsQueryHandler> logger
) : IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>
{
    public async Task<IReadOnlyCollection<UserGroupDto>> Handle(
        UserGroupsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User groups query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : actor.PartitionAccessesGuids;

        var userGroups = await userGroupRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            query.NotInsideAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User groups query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.Guid,
                query.InsideAnyOfThisPartitions?.Count ?? 0,
                insideAnyOfThisPartitions?.Count ?? 0,
                userGroups.Count);
        }

        return userGroups;
    }
}

#endregion Many
