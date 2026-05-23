using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

#region Create

/// <summary>
/// Command used to create a new user group.
/// </summary>
/// <param name="Nameid">
/// User group login identifier.
/// </param>
public sealed record UserGroupCreateCommand(
    Nameid Nameid
) : ICommand<Guid>;

/// <summary>
/// Handles user group creation.
/// </summary>
/// <remarks>
/// Validates permissions, validates domain rules,
/// and stores the new user group.
/// </remarks>
public sealed class UserGroupCreateCommandHandler(
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupCreateCommandHandler> logger
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User group create flow started by actor {ActorGuid}.", actor.Guid);
        }

        var userGroup = UserGroup.CreateUserGroup(command.Nameid, actor);

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        userGroupRepository.Add(userGroup);

        entityEventRepository.Add(EntityEvent.EntityCreated<UserGroup>(userGroup, actor.Guid));

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

#region Delete

/// <summary>
/// Command used to delete a user group.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;

/// <summary>
/// Handles user group deletion.
/// </summary>
/// <remarks>
/// Validates permissions and user group deletion rules
/// before removing the user group.
/// </remarks>
public sealed class UserGroupDeleteCommandHandler(
    IUserGroupRepository userGroupRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupDeleteCommandHandler> logger
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task Handle(
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

        entityEventRepository.Add(EntityEvent.EntityDeleted<UserGroup>(userGroup, actor.Guid));

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

#region Nameid

/// <summary>
/// Command used to change a user group login identifier.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
/// <param name="Nameid">
/// New user group login identifier.
/// </param>
public sealed record UserGroupChangeNameidCommand(Guid UserGroupGuid, Nameid Nameid) : ICommand;

/// <summary>
/// Handles user group login identifier changes.
/// </summary>
/// <remarks>
/// Validates permissions and user group nameid uniqueness rules.
/// </remarks>
public sealed class UserGroupChangeNameidCommandHandler(
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupChangeNameidCommandHandler> logger) : ICommandHandler<UserGroupChangeNameidCommand>
{
    public async Task Handle(UserGroupChangeNameidCommand command, CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group nameid change flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.Guid);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        if (userGroup.Nameid == command.Nameid)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group nameid change flow skipped for user group {UserGroupGuid}; nameid is already requested value.",
                    userGroup.Guid);
            }

            return;
        }

        await userGroupService.ValidateUserGroupNameidChange(userGroup, command.Nameid, cancellationToken);

        userGroup.ChangeNameid(command.Nameid, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group nameid change mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }
    }
}

#endregion Nameid

#region Description

/// <summary>
/// Command used to change a user group description.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
/// <param name="Description">
/// New user group description.
/// </param>
public sealed record UserGroupChangeDescriptionCommand(
    Guid UserGroupGuid,
    Description Description
) : ICommand;

/// <summary>
/// Handles user group description changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the description.
/// </remarks>
public sealed class UserGroupChangeDescriptionCommandHandler(
    IUserGroupRepository userGroupRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupChangeDescriptionCommandHandler> logger
) : ICommandHandler<UserGroupChangeDescriptionCommand>
{
    public async Task Handle(UserGroupChangeDescriptionCommand command, CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group description change flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.Guid);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        if (userGroup.Description == command.Description)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group description change flow skipped for user group {UserGroupGuid}; description is already requested value.",
                    userGroup.Guid);
            }

            return;
        }

        userGroup.ChangeDescription(command.Description, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group description change mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }
    }
}

#endregion Description

#region Permission

/// <summary>
/// Command used to replace the user group permissions.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
/// <param name="Actions">
/// Desired permission actions.
/// </param>
public sealed record UserGroupSetPermissionsCommand(
    Guid UserGroupGuid,
    IReadOnlyCollection<ActionType> Actions
) : ICommand;

/// <summary>
/// Handles user group permission changes.
/// </summary>
/// <remarks>
/// Validates permissions and synchronizes the user group permissions
/// with the requested set.
/// </remarks>
public sealed class UserGroupSetPermissionsCommandHandler(
    IUserGroupRepository userGroupRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupSetPermissionsCommandHandler> logger) : ICommandHandler<UserGroupSetPermissionsCommand>
{
    public async Task Handle(UserGroupSetPermissionsCommand command, CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group permission mutation started for user group {UserGroupGuid} by actor {ActorGuid}. RequestedPermissionCount: {RequestedPermissionCount}.",
                command.UserGroupGuid,
                actor.Guid,
                command.Actions.Count);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        var requestedActions = command.Actions.Distinct().ToHashSet();

        var currentActions = userGroup.Permissions.Select(x => x.Action).ToHashSet();

        if (requestedActions.SetEquals(currentActions))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group permission mutation skipped for user group {UserGroupGuid}; permissions are already requested values.",
                    userGroup.Guid);
            }
            return;
        }

        foreach (var action in requestedActions.Except(currentActions))
        {
            userGroup.AddPermission(action, actor);
        }

        foreach (var action in currentActions.Except(requestedActions))
        {
            userGroup.RemovePermission(action, actor);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group permission mutation completed for user group {UserGroupGuid} by actor {ActorGuid}. PermissionCount: {PermissionCount}.",
                userGroup.Guid,
                actor.Guid,
                userGroup.Permissions.Count);
        }
    }
}

#endregion Permission

#region Partition

/// <summary>
/// Command used to replace the user group partition assignments.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
/// <param name="PartitionGuids">
/// Desired partition unique identifiers.
/// </param>
public sealed record UserGroupSetPartitionsCommand(Guid UserGroupGuid, IReadOnlyCollection<Guid> PartitionGuids) : ICommand;

/// <summary>
/// Handles user group partition assignment changes.
/// </summary>
/// <remarks>
/// Validates permissions and synchronizes the user group partitions
/// with the requested set.
/// </remarks>
public sealed class UserGroupSetPartitionsCommandHandler(
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupSetPartitionsCommandHandler> logger) : ICommandHandler<UserGroupSetPartitionsCommand>
{
    public async Task Handle(UserGroupSetPartitionsCommand command, CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group partition mutation started for user group {UserGroupGuid} by actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}.",
                command.UserGroupGuid,
                actor.Guid,
                command.PartitionGuids.Count);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        var requestedPartitions = command.PartitionGuids.Distinct().ToArray();

        var hasChanges =
            userGroup.Partitions.Count != requestedPartitions.Length ||
            userGroup.Partitions.Any(p => !requestedPartitions.Contains(p.Guid));

        if (!hasChanges)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group partition mutation skipped for user group {UserGroupGuid}; partitions are already requested values.",
                    userGroup.Guid);
            }

            return;
        }

        foreach (var partitionGuid in requestedPartitions)
        {
            if (userGroup.Partitions.Any(p => p.Guid == partitionGuid))
            {
                continue;
            }

            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            userGroup.AddPartition(partition, actor);

            entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                userGroup,
                partition,
                actor.Guid));
        }

        var partitionsToRemove = userGroup.Partitions.Where(p => !requestedPartitions.Contains(p.Guid)).ToList();

        foreach (var partition in partitionsToRemove)
        {
            userGroup.RemovePartition(partition, actor);

            entityPartitionEventRepository.Add(EntityPartitionEvent.RemovedFromPartition(
                userGroup,
                partition,
                actor.Guid));
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group partition mutation completed for user group {UserGroupGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                userGroup.Guid,
                actor.Guid,
                userGroup.Partitions.Count);
        }
    }
}

#endregion Partition

#region Activate

/// <summary>
/// Command used to activate a user group.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
public sealed record UserGroupActivateCommand(Guid UserGroupGuid) : ICommand;

/// <summary>
/// Handles user group activation.
/// </summary>
/// <remarks>
/// Validates permissions and activates the user group.
/// </remarks>
public sealed class UserGroupActivateCommandHandler(
    IUserGroupRepository userGroupRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupActivateCommandHandler> logger) : ICommandHandler<UserGroupActivateCommand>
{
    public async Task Handle(UserGroupActivateCommand command, CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group activation flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.Guid);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        if (userGroup.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group activation flow skipped for user group {UserGroupGuid}; user group is already active.",
                    userGroup.Guid);
            }

            return;
        }

        userGroup.Activate(actor);

        entityEventRepository.Add(EntityEvent.Activated<UserGroup>(userGroup, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group activation mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }
    }
}

#endregion Activate

#region Deactivate

/// <summary>
/// Command used to deactivate a user group.
/// </summary>
/// <param name="UserGroupGuid">
/// User group unique identifier.
/// </param>
public sealed record UserGroupDeactivateCommand(Guid UserGroupGuid) : ICommand;

/// <summary>
/// Handles user group deactivation.
/// </summary>
/// <remarks>
/// Validates permissions and deactivates the user group.
/// </remarks>
public sealed class UserGroupDeactivateCommandHandler(
    IUserGroupRepository userGroupRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserGroupDeactivateCommandHandler> logger) : ICommandHandler<UserGroupDeactivateCommand>
{
    public async Task Handle(UserGroupDeactivateCommand command, CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group deactivation flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.Guid);
        }

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        userGroup.ValidateCanEdit(actor);

        if (!userGroup.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group deactivation flow skipped for user group {UserGroupGuid}; user group is already inactive.",
                    userGroup.Guid);
            }
            return;
        }

        userGroup.Deactivate(actor);

        entityEventRepository.Add(EntityEvent.Deactivated<UserGroup>(userGroup, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group deactivation mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.Guid);
        }
    }
}

#endregion Deactivate
