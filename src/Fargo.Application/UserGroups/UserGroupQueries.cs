using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.UserGroups;

#region Single

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserGroupDto?>;

public sealed class UserGroupSingleQueryHandler(
    ActorService actorService,
    IUserGroupQueryRepository userGroupRepository,
    ICurrentUser currentUser
) : IQueryHandler<UserGroupSingleQuery, UserGroupDto?>
{
    public async Task<UserGroupDto?> Handle(
        UserGroupSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var userGroup = await userGroupRepository.GetInfoByGuid(
            query.UserGroupGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notInsideAnyPartition: null,
            cancellationToken
        );

        if (userGroup is not null && userGroup.Partitions.Any(p => !actor.PartitionAccessesGuids.Contains(p)))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
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
    ICurrentUser currentUser
) : IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>
{
    public async Task<IReadOnlyCollection<UserGroupDto>> Handle(
        UserGroupsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var insideAnyOfThisPartitions = query.InsideAnyOfThisPartitions is { } requested
            ? [.. actor.PartitionAccessesGuids.Intersect(requested)]
            : actor.PartitionAccessesGuids;

        var userGroups = await userGroupRepository.GetManyInfo(
            query.WithPagination,
            query.TemporalAsOfDateTime,
            insideAnyOfThisPartitions,
            query.NotInsideAnyPartition,
            cancellationToken
        );

        if (userGroups.Any(g => g.Partitions.Any(p => !actor.PartitionAccessesGuids.Contains(p))))
        {
            throw new EntityAccessViolationFargoApplicationException(actor.Guid);
        }

        return userGroups;
    }
}

#endregion Many
