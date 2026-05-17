using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User group create flow started by actor {ActorGuid}.", actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateUserGroup);

        var userGroup = new UserGroup(command.Nameid);

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.General);

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        userGroupRepository.Add(userGroup);

        entityEventRepository.Add(EntityEvent.EntityCreated<UserGroup>(userGroup, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group create mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
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

        entityEventRepository.Add(EntityEvent.EntityDeleted<UserGroup>(userGroup, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group nameid change flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

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

        userGroup.ChangeNameid(command.Nameid);

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group nameid change mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group description change flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

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

        userGroup.ChangeDescription(command.Description);

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group description change mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group permission mutation started for user group {UserGroupGuid} by actor {ActorGuid}. RequestedPermissionCount: {RequestedPermissionCount}.",
                command.UserGroupGuid,
                actor.ActorGuid,
                command.Actions.Count);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

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
            userGroup.AddPermission(action);
        }

        foreach (var action in currentActions.Except(requestedActions))
        {
            userGroup.RemovePermission(action);
        }

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.PermissionsChanged);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group permission mutation completed for user group {UserGroupGuid} by actor {ActorGuid}. PermissionCount: {PermissionCount}.",
                userGroup.Guid,
                actor.ActorGuid,
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group partition mutation started for user group {UserGroupGuid} by actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}.",
                command.UserGroupGuid,
                actor.ActorGuid,
                command.PartitionGuids.Count);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

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

            actor.ValidateHasPartitionAccess(partition.Guid);

            userGroup.AddPartition(partition);

            entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                userGroup,
                partition,
                actor.ActorGuid));
        }

        var partitionsToRemove = userGroup.Partitions.Where(p => !requestedPartitions.Contains(p.Guid)).ToList();

        foreach (var partition in partitionsToRemove)
        {
            actor.ValidateHasPartitionAccess(partition.Guid);
            userGroup.RemovePartition(partition);

            entityPartitionEventRepository.Add(EntityPartitionEvent.RemovedFromPartition(
                userGroup,
                partition,
                actor.ActorGuid));
        }

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.PartitionsChanged);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group partition mutation completed for user group {UserGroupGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                userGroup.Guid,
                actor.ActorGuid,
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group activation flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

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

        userGroup.Activate();

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.Activated);

        entityEventRepository.Add(EntityEvent.Activated<UserGroup>(userGroup, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group activation mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
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
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group deactivation flow started for user group {UserGroupGuid} by actor {ActorGuid}.",
                command.UserGroupGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

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

        userGroup.Deactivate();

        userGroup.MarkAsEditedBy(actor.ActorGuid);

        userGroup.MarkModificationType(UserGroupModifiedType.Deactivated);

        entityEventRepository.Add(EntityEvent.Deactivated<UserGroup>(userGroup, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group deactivation mutation completed for user group {UserGroupGuid} by actor {ActorGuid}.",
                userGroup.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Deactivate
