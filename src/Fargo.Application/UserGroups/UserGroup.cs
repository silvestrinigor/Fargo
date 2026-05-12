using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Fargo.Application.UserGroups;

#region DTOs

public sealed record UserGroupDto(
    Guid Guid,
    Nameid Nameid,
    Description Description,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record UserGroupCreateDto(
    string Nameid,
    Description? Description = null,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions = null,
    IReadOnlyCollection<Guid>? Partitions = null
);

public sealed record UserGroupUpdateDto(
    string? Nameid,
    Description? Description,
    bool? IsActive,
    IReadOnlyCollection<UserGroupPermissionUpdateDto>? Permissions,
    IReadOnlyCollection<Guid>? Partitions
);

public sealed record UserGroupPermissionUpdateDto(
    ActionType Action
);

public static class UserGroupDtoMappings
{
    public static readonly Expression<Func<UserGroup, UserGroupDto>> Projection = userGroup => new UserGroupDto(
        userGroup.Guid,
        userGroup.Nameid,
        userGroup.Description,
        userGroup.Permissions.Select(permission => new Permission(permission.Guid, permission.Action)).ToArray(),
        userGroup.Partitions.Select(partition => partition.Guid).ToArray(),
        userGroup.IsActive,
        userGroup.EditedByGuid);
}

#endregion DTOs

#region Exceptions

public sealed class UserGroupNotFoundFargoApplicationException(Guid userGroupGuid)
    : FargoApplicationException($"User group '{userGroupGuid}' was not found.")
{
    public Guid UserGroupGuid { get; } = userGroupGuid;
}

#endregion Exceptions

#region Repositories

public interface IUserGroupQueryRepository
{
    Task<UserGroupDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserGroupDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}
public static class UserGroupRepositoryExtensions
{
    extension(IUserGroupRepository repository)
    {
        public async Task<UserGroup> GetFoundByGuid(
            Guid userGroupGuid,
            CancellationToken cancellationToken = default
        )
        {
            var group = await repository.GetByGuid(userGroupGuid, cancellationToken)
                ?? throw new UserGroupNotFoundFargoApplicationException(userGroupGuid);

            return group;
        }
    }
}

#endregion Repositories

#region Create Delete Update

#region Create

public sealed record UserGroupCreateCommand(
    UserGroupCreateDto UserGroup
) : ICommand<Guid>;

public sealed class UserGroupCreateCommandHandler(
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupCreateCommandHandler> logger
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User group create flow started by actor {ActorGuid}.", actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateUserGroup);

        var nameid = ValidateNameid(command.UserGroup.Nameid);

        var userGroup = new UserGroup
        {
            Nameid = nameid,
            Description = command.UserGroup.Description ?? Description.Empty
        };

        #region Partition

        foreach (var partitionGuid in command.UserGroup.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            userGroup.Partitions.Add(partition);
        }

        #endregion Partition

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        foreach (var permission in command.UserGroup.Permissions ?? [])
        {
            userGroup.AddPermission(permission.Action);
        }

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group create flow completed for user group {UserGroupGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}. PermissionCount: {PermissionCount}.",
                userGroup.Guid,
                actor.ActorGuid,
                userGroup.Partitions.Count,
                userGroup.Permissions.Count);
        }

        return userGroup.Guid;
    }

    private static Nameid ValidateNameid(string value)
    {
        try
        {
            return new Nameid(value);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidNameidFargoApplicationException(ex.Message);
        }
    }
}

#endregion Create

#region Delete

public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;

public sealed class UserGroupDeleteCommandHandler(
    IUserGroupRepository userGroupRepository,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupDeleteCommandHandler> logger
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task Handle(
        UserGroupDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.DeleteUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        if (actor.UserGroupGuids.Contains(userGroup.Guid))
        {
            throw new UserCannotDeleteParentUserGroupFargoDomainException(userGroup.Guid);
        }

        UserGroupService.ValidateUserGroupDelete(userGroup);

        userGroupRepository.Remove(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete flow completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Update

public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid,
    UserGroupUpdateDto UserGroup
) : ICommand;

public sealed class UserGroupUpdateCommandHandler(
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupUpdateCommandHandler> logger
) : ICommandHandler<UserGroupUpdateCommand>
{
    public async Task Handle(
        UserGroupUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group update flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        if (command.UserGroup.Nameid is not null)
        {
            var nameid = ValidateNameid(command.UserGroup.Nameid);

            if (userGroup.Nameid != nameid)
            {
                await userGroupService.ValidateUserGroupNameidChange(userGroup, nameid, cancellationToken);
                userGroup.Nameid = nameid;
            }
        }

        if (command.UserGroup.Description is not null && userGroup.Description != command.UserGroup.Description)
        {
            userGroup.Description = command.UserGroup.Description.Value;
        }

        if (command.UserGroup.IsActive is not null && userGroup.IsActive != command.UserGroup.IsActive.Value)
        {
            if (command.UserGroup.IsActive.Value)
            {
                userGroup.Activate();
            }
            else
            {
                userGroup.Deactivate();
            }
        }

        if (command.UserGroup.Permissions is not null)
        {
            var requestedActions = command.UserGroup.Permissions
                .Select(x => x.Action)
                .Distinct()
                .ToHashSet();

            var currentActions = userGroup.Permissions
                .Select(x => x.Action)
                .ToHashSet();

            foreach (var action in requestedActions.Except(currentActions))
            {
                userGroup.AddPermission(action);
            }

            foreach (var action in currentActions.Except(requestedActions))
            {
                userGroup.RemovePermission(action);
            }
        }

        #region Partition

        if (command.UserGroup.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (userGroup.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                userGroup.Partitions.Add(partition);
            }

            var partitionsToRemove = userGroup.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                userGroup.Partitions.Remove(partition);
            }
        }

        #endregion Partition

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group update flow completed for user group {UserGroupGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}. PermissionCount: {PermissionCount}.",
                userGroup.Guid,
                actor.ActorGuid,
                userGroup.Partitions.Count,
                userGroup.Permissions.Count);
        }
    }

    private static Nameid ValidateNameid(string value)
    {
        try
        {
            return new Nameid(value);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidNameidFargoApplicationException(ex.Message);
        }
    }
}

#endregion Update

#endregion Create Delete Update

#region Queries

#region Single

public sealed record UserGroupSingleQuery(
    Guid UserGroupGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserGroupDto?>;

public sealed class UserGroupSingleQueryHandler(
    IUserGroupQueryRepository userGroupRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupSingleQueryHandler> logger
) : IQueryHandler<UserGroupSingleQuery, UserGroupDto?>
{
    public async Task<UserGroupDto?> Handle(
        UserGroupSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User group single query started for user group {UserGroupGuid} by actor {ActorGuid}.",
                query.UserGroupGuid,
                actor.ActorGuid);
        }

        var userGroup = await userGroupRepository.GetInfoByGuid(
            query.UserGroupGuid,
            query.AsOfDateTime,
            actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User group single query completed for user group {UserGroupGuid} by actor {ActorGuid}. Found: {Found}.",
                query.UserGroupGuid,
                actor.ActorGuid,
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
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<UserGroupDto>>;

public sealed class UserGroupsQueryHandler(
    IUserGroupQueryRepository userGroupRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupsQueryHandler> logger
) : IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>>
{
    public async Task<IReadOnlyCollection<UserGroupDto>> Handle(
        UserGroupsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User groups query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actor.ActorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccesses,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var userGroups = await userGroupRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User groups query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.ActorGuid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                userGroups.Count);
        }

        return userGroups;
    }
}

#endregion Many

#endregion Queries
