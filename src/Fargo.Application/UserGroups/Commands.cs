using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.UserGroups;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

#region Create

/// <summary>
/// Command used to create a new user group from an API creation payload.
/// </summary>
public sealed record UserGroupCreateCommand(
    UserGroupCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Handles user group creation, including optional create-time state.
/// </summary>
public sealed class UserGroupCreateCommandHandler(
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IEventRepository entityEventRepository,
    IPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupCreateCommandHandler> logger
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var create = command.Create;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User group create flow started by actor {ActorGuid}.", actor.Guid);
        }

        Nameid nameid;

        try
        {
            nameid = new Nameid(create.Nameid);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidNameidFargoApplicationException(ex.Message);
        }

        var userGroup = UserGroup.CreateUserGroup(nameid, actor);

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        userGroupRepository.Add(userGroup);

        entityEventRepository.Add(Event.NewEntityCreatedEvent<UserGroup>(userGroup, actor.Guid));

        if (create.Description is { } description)
        {
            userGroup.ChangeDescription(description, actor);
        }

        if (create.Permissions is { } permissions)
        {
            userGroup.ValidateCanEdit(actor);

            var requestedActions = permissions.Select(p => p.Action).Distinct().ToHashSet();
            var currentActions = userGroup.Permissions.Select(p => p.Action).ToHashSet();

            foreach (var action in requestedActions.Except(currentActions))
            {
                userGroup.AddPermission(action, actor);
            }

            foreach (var action in currentActions.Except(requestedActions))
            {
                userGroup.RemovePermission(action, actor);
            }
        }

        if (create.Partitions is { Count: > 0 } partitions)
        {
            userGroup.ValidateCanEdit(actor);

            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                userGroup.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(PartitionEvent.InsertedIntoPartition(
                    userGroup,
                    partition,
                    actor.Guid));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group create mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }

        return userGroup.Guid;
    }
}

#endregion Create

#region Update

/// <summary>
/// Command used to update an existing user group from an API update payload.
/// </summary>
public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid,
    UserGroupUpdateDto Update
) : ICommand;

/// <summary>
/// Handles user group updates.
/// </summary>
public sealed class UserGroupUpdateCommandHandler(
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IEventRepository entityEventRepository,
    IPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupUpdateCommandHandler> logger
) : ICommandHandler<UserGroupUpdateCommand>
{
    public async Task HandleAsync(
        UserGroupUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var update = command.Update;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group update flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.Guid);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        if (update.Nameid is not null)
        {
            Nameid nameid;

            try
            {
                nameid = new Nameid(update.Nameid);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidNameidFargoApplicationException(ex.Message);
            }

            if (userGroup.Nameid != nameid)
            {
                await userGroupService.ValidateUserGroupNameidChange(userGroup, nameid, cancellationToken);
                userGroup.ChangeNameid(nameid, actor);
            }
        }

        if (update.Description is { } description)
        {
            userGroup.ChangeDescription(description, actor);
        }

        if (update.Permissions is { } permissions)
        {
            var requestedActions = permissions.Select(p => p.Action).Distinct().ToHashSet();
            var currentActions = userGroup.Permissions.Select(p => p.Action).ToHashSet();

            foreach (var action in requestedActions.Except(currentActions))
            {
                userGroup.AddPermission(action, actor);
            }

            foreach (var action in currentActions.Except(requestedActions))
            {
                userGroup.RemovePermission(action, actor);
            }
        }

        if (update.Partitions is { } partitions)
        {
            var requestedPartitionGuids = partitions.Distinct().ToArray();

            foreach (var partitionGuid in requestedPartitionGuids)
            {
                if (userGroup.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                userGroup.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(PartitionEvent.InsertedIntoPartition(
                    userGroup,
                    partition,
                    actor.Guid));
            }

            var partitionsToRemove = userGroup.Partitions
                .Where(p => !requestedPartitionGuids.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                userGroup.RemovePartition(partition, actor);

                entityPartitionEventRepository.Add(PartitionEvent.RemovedFromPartition(
                    userGroup,
                    partition,
                    actor.Guid));
            }
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive && !userGroup.IsActive)
            {
                userGroup.Activate(actor);
                entityEventRepository.Add(Event.Activated<UserGroup>(userGroup, actor.Guid));
            }
            else if (!isActive && userGroup.IsActive)
            {
                userGroup.Deactivate(actor);
                entityEventRepository.Add(Event.Deactivated<UserGroup>(userGroup, actor.Guid));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group update mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }
    }
}

#endregion Update

#region Delete

/// <summary>
/// Command used to delete a user group.
/// </summary>
public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;

/// <summary>
/// Handles user group deletion.
/// </summary>
public sealed class UserGroupDeleteCommandHandler(
    IUserGroupRepository userGroupRepository,
    IEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupDeleteCommandHandler> logger
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task HandleAsync(
        UserGroupDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.Guid);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        UserGroupService.ValidateUserGroupDelete(userGroup, actor, authorizationContext.UserGroupGuids);

        userGroupRepository.Remove(userGroup);

        entityEventRepository.Add(Event.EntityDeleted<UserGroup>(userGroup, actor.Guid));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete
