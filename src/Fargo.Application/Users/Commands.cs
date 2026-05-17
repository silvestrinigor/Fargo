using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Core;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

#region Create

/// <summary>
/// Command used to create a new user.
/// </summary>
/// <param name="Nameid">
/// User login identifier.
/// </param>
/// <param name="Password">
/// Initial user password.
/// </param>
public sealed record UserCreateCommand(
    Nameid Nameid,
    Password Password
) : ICommand<Guid>;

/// <summary>
/// Handles user creation.
/// </summary>
/// <remarks>
/// Validates permissions, hashes the password,
/// validates domain rules, and stores the new user.
/// </remarks>
public sealed class UserCreateCommandHandler(
    UserService userService,
    IUserRepository userRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IPasswordHasher passwordHasher,
    ILogger<UserCreateCommandHandler> logger
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        UserCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User create flow started by actor {ActorGuid}.", actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateUser);

        var userPasswordHash = passwordHasher.Hash(command.Password);

        var user = new User(command.Nameid, userPasswordHash);
        user.MarkPasswordChangeAsRequired();

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.General);

        await userService.ValidateUserCreate(user, cancellationToken);

        userRepository.Add(user);

        entityEventRepository.Add(EntityEvent.EntityCreated<User>(user, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User create mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }

        return user.Guid;
    }

}

#endregion Create

#region Delete

/// <summary>
/// Command used to delete a user.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
public sealed record UserDeleteCommand(
    Guid UserGuid
) : ICommand;

/// <summary>
/// Handles user deletion.
/// </summary>
/// <remarks>
/// Validates permissions and user deletion rules
/// before removing the user.
/// </remarks>
public sealed class UserDeleteCommandHandler(
    IUserRepository userRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserDeleteCommandHandler> logger
) : ICommandHandler<UserDeleteCommand>
{
    public async Task Handle(
        UserDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.DeleteUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        UserService.ValidateUserDelete(user, actor.ActorGuid);

        userRepository.Remove(user);

        entityEventRepository.Add(EntityEvent.EntityDeleted<User>(user, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Nameid

/// <summary>
/// Command used to change a user login identifier.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Nameid">
/// New user login identifier.
/// </param>
public sealed record UserChangeNameidCommand(
    Guid UserGuid,
    Nameid Nameid
) : ICommand;

/// <summary>
/// Handles user login identifier changes.
/// </summary>
/// <remarks>
/// Validates permissions and user nameid uniqueness rules.
/// </remarks>
public sealed class UserChangeNameidCommandHandler(
    UserService userService,
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserChangeNameidCommandHandler> logger
) : ICommandHandler<UserChangeNameidCommand>
{
    public async Task Handle(UserChangeNameidCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User nameid change flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (user.Nameid == command.Nameid)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User nameid change flow skipped for user {UserGuid}; nameid is already requested value.",
                    user.Guid);
            }

            return;
        }

        await userService.ValidateUserNameidChange(user, command.Nameid, cancellationToken);

        user.ChangeNameid(command.Nameid);

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User nameid change mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Nameid

#region FirstName

/// <summary>
/// Command used to change a user first name.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="FirstName">
/// New user first name.
/// </param>
public sealed record UserChangeFirstNameCommand(
    Guid UserGuid,
    FirstName? FirstName
) : ICommand;

/// <summary>
/// Handles user first name changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the first name.
/// </remarks>
public sealed class UserChangeFirstNameCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserChangeFirstNameCommandHandler> logger) : ICommandHandler<UserChangeFirstNameCommand>
{
    public async Task Handle(UserChangeFirstNameCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User first name change flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (user.FirstName == command.FirstName)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User first name change flow skipped for user {UserGuid}; first name is already requested value.",
                    user.Guid);
            }
            return;
        }

        user.ChangeFirstName(command.FirstName);

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User first name change mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion FirstName

#region LastName

/// <summary>
/// Command used to change a user last name.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="LastName">
/// New user last name.
/// </param>
public sealed record UserChangeLastNameCommand(
    Guid UserGuid,
    LastName? LastName
) : ICommand;

/// <summary>
/// Handles user last name changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the last name.
/// </remarks>
public sealed class UserChangeLastNameCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserChangeLastNameCommandHandler> logger) : ICommandHandler<UserChangeLastNameCommand>
{
    public async Task Handle(UserChangeLastNameCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User last name change flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (user.LastName == command.LastName)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User last name change flow skipped for user {UserGuid}; last name is already requested value.",
                    user.Guid);
            }

            return;
        }

        user.ChangeLastName(command.LastName);

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User last name change mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion LastName

#region Description

/// <summary>
/// Command used to change a user description.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Description">
/// New user description.
/// </param>
public sealed record UserChangeDescriptionCommand(
    Guid UserGuid,
    Description Description
) : ICommand;

/// <summary>
/// Handles user description changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the description.
/// </remarks>
public sealed class UserChangeDescriptionCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserChangeDescriptionCommandHandler> logger) : ICommandHandler<UserChangeDescriptionCommand>
{
    public async Task Handle(UserChangeDescriptionCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User description change flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (user.Description == command.Description)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User description change flow skipped for user {UserGuid}; description is already requested value.",
                    user.Guid);
            }

            return;
        }

        user.ChangeDescription(command.Description);

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User description change mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Description

#region DefaultPasswordExpiration

/// <summary>
/// Command used to set the default user password expiration period.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Period">
/// Default password expiration period.
/// </param>
public sealed record UserSetDefaultPasswordExpirationCommand(Guid UserGuid, TimeSpan? Period) : ICommand;

/// <summary>
/// Handles default password expiration period changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the user password expiration rule.
/// </remarks>
public sealed class UserSetDefaultPasswordExpirationCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserSetDefaultPasswordExpirationCommandHandler> logger) : ICommandHandler<UserSetDefaultPasswordExpirationCommand>
{
    public async Task Handle(UserSetDefaultPasswordExpirationCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User default password expiration mutation started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (user.DefaultPasswordExpirationPeriod == command.Period)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User default password expiration mutation skipped for user {UserGuid}; period is already requested value.",
                    user.Guid);
            }

            return;
        }

        user.SetDefaultPasswordExpirationPeriod(command.Period);

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.General);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User default password expiration mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion DefaultPasswordExpiration

#region Password

/// <summary>
/// Command used to change a user password.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Password">
/// New user password.
/// </param>
public sealed record UserChangePasswordCommand(Guid UserGuid, Password Password) : ICommand;

/// <summary>
/// Handles user password changes.
/// </summary>
/// <remarks>
/// Validates permissions, hashes the new password,
/// requires a password change, and revokes active refresh tokens.
/// </remarks>
public sealed class UserChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserChangePasswordCommandHandler> logger) : ICommandHandler<UserChangePasswordCommand>
{
    public async Task Handle(UserChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User password change flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        actor.ValidateHasPermission(ActionType.ChangeOtherUserPassword);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        user.ChangePasswordHash(passwordHasher.Hash(command.Password));

        user.MarkPasswordChangeAsRequired();

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.PasswordChanged);

        var refreshTokens = await refreshTokenRepository.GetByUserGuid(user.Guid, cancellationToken);

        var usableRefreshTokens = refreshTokens.Where(refreshToken => refreshToken.IsUsable).ToArray();

        foreach (var refreshToken in usableRefreshTokens)
        {
            refreshToken.Revoke();
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User password change mutation completed for user {UserGuid} by actor {ActorGuid}. RevokedRefreshTokenCount: {RevokedRefreshTokenCount}.",
                user.Guid,
                actor.ActorGuid,
                usableRefreshTokens.Length);
        }
    }
}

#endregion Password

#region Permission

/// <summary>
/// Command used to replace the user permissions.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Actions">
/// Desired permission actions.
/// </param>
public sealed record UserSetPermissionsCommand(Guid UserGuid, IReadOnlyCollection<ActionType> Actions) : ICommand;

/// <summary>
/// Handles user permission changes.
/// </summary>
/// <remarks>
/// Validates permissions and synchronizes the user permissions
/// with the requested set.
/// </remarks>
public sealed class UserSetPermissionsCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserSetPermissionsCommandHandler> logger) : ICommandHandler<UserSetPermissionsCommand>
{
    public async Task Handle(UserSetPermissionsCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User permission mutation started for user {UserGuid} by actor {ActorGuid}. RequestedPermissionCount: {RequestedPermissionCount}.",
                command.UserGuid,
                actor.ActorGuid,
                command.Actions.Count);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        var requestedActions = command.Actions.Distinct().ToHashSet();

        var currentActions = user.Permissions.Select(x => x.Action).ToHashSet();

        if (requestedActions.SetEquals(currentActions))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User permission mutation skipped for user {UserGuid}; permissions are already requested values.",
                    user.Guid);
            }
            return;
        }

        UserService.ValidateUserPermissionChange(user, actor.ActorGuid);

        foreach (var action in requestedActions.Except(currentActions))
        {
            user.AddPermission(action);
        }

        foreach (var action in currentActions.Except(requestedActions))
        {
            user.RemovePermission(action);
        }

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.PermissionsChanged);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User permission mutation completed for user {UserGuid} by actor {ActorGuid}. PermissionCount: {PermissionCount}.",
                user.Guid,
                actor.ActorGuid,
                user.Permissions.Count);
        }
    }
}

#endregion Permission

#region Partition

/// <summary>
/// Command used to replace the user partition assignments.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="PartitionGuids">
/// Desired partition unique identifiers.
/// </param>
public sealed record UserSetPartitionsCommand(
    Guid UserGuid,
    IReadOnlyCollection<Guid> PartitionGuids
) : ICommand;

/// <summary>
/// Handles user partition assignment changes.
/// </summary>
/// <remarks>
/// Validates permissions and synchronizes the user partitions
/// with the requested set.
/// </remarks>
public sealed class UserSetPartitionsCommandHandler(
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserSetPartitionsCommandHandler> logger) : ICommandHandler<UserSetPartitionsCommand>
{
    public async Task Handle(UserSetPartitionsCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User partition mutation started for user {UserGuid} by actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}.",
                command.UserGuid,
                actor.ActorGuid,
                command.PartitionGuids.Count);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        var requestedPartitions = command.PartitionGuids.Distinct().ToArray();

        var hasChanges =
            user.Partitions.Count != requestedPartitions.Length ||
            user.Partitions.Any(p => !requestedPartitions.Contains(p.Guid));

        if (!hasChanges)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User partition mutation skipped for user {UserGuid}; partitions are already requested values.",
                    user.Guid);
            }
            return;
        }

        foreach (var partitionGuid in requestedPartitions)
        {
            if (user.Partitions.Any(p => p.Guid == partitionGuid))
            {
                continue;
            }

            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            user.AddPartition(partition);
        }

        var partitionsToRemove = user.Partitions.Where(p => !requestedPartitions.Contains(p.Guid)).ToList();

        foreach (var partition in partitionsToRemove)
        {
            actor.ValidateHasPartitionAccess(partition.Guid);
            user.RemovePartition(partition);
        }

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.PartitionsChanged);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User partition mutation completed for user {UserGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                user.Guid,
                actor.ActorGuid,
                user.Partitions.Count);
        }
    }
}

#endregion Partition

#region UserGroup

/// <summary>
/// Command used to replace the user group assignments for a user.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="UserGroupGuids">
/// Desired user group unique identifiers.
/// </param>
public sealed record UserSetUserGroupsCommand(Guid UserGuid, IReadOnlyCollection<Guid> UserGroupGuids) : ICommand;

/// <summary>
/// Handles user group assignment changes for a user.
/// </summary>
/// <remarks>
/// Validates permissions and synchronizes the user groups
/// with the requested set.
/// </remarks>
public sealed class UserSetUserGroupsCommandHandler(
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserSetUserGroupsCommandHandler> logger) : ICommandHandler<UserSetUserGroupsCommand>
{
    public async Task Handle(UserSetUserGroupsCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group assignment mutation started for user {UserGuid} by actor {ActorGuid}. RequestedUserGroupCount: {RequestedUserGroupCount}.",
                command.UserGuid,
                actor.ActorGuid,
                command.UserGroupGuids.Count);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        var requestedUserGroups = command.UserGroupGuids.Distinct().ToArray();

        var hasChanges =
            user.UserGroups.Count != requestedUserGroups.Length ||
            user.UserGroups.Any(g => !requestedUserGroups.Contains(g.Guid));

        if (!hasChanges)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User group assignment mutation skipped for user {UserGuid}; user groups are already requested values.",
                    user.Guid);
            }
            return;
        }

        foreach (var userGroupGuid in requestedUserGroups)
        {
            if (user.UserGroups.Any(g => g.Guid == userGroupGuid))
            {
                continue;
            }

            var userGroup = await userGroupRepository.GetFoundByGuid(userGroupGuid, cancellationToken);

            actor.ValidateHasAccess(userGroup);

            if (!userGroup.IsActive)
            {
                throw new UserGroupInactiveFargoDomainException(userGroup.Guid);
            }

            user.AddUserGroup(userGroup);
        }

        var userGroupsToRemove = user.UserGroups.Where(g => !requestedUserGroups.Contains(g.Guid)).ToList();

        foreach (var userGroup in userGroupsToRemove)
        {
            actor.ValidateHasAccess(userGroup);
            user.RemoveUserGroup(userGroup);
        }

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.UserGroupsChanged);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group assignment mutation completed for user {UserGuid} by actor {ActorGuid}. UserGroupCount: {UserGroupCount}.",
                user.Guid,
                actor.ActorGuid,
                user.UserGroups.Count);
        }
    }
}

#endregion UserGroup

#region Activate

/// <summary>
/// Command used to activate a user.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
public sealed record UserActivateCommand(Guid UserGuid) : ICommand;

/// <summary>
/// Handles user activation.
/// </summary>
/// <remarks>
/// Validates permissions and activates the user.
/// </remarks>
public sealed class UserActivateCommandHandler(
    IUserRepository userRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserActivateCommandHandler> logger) : ICommandHandler<UserActivateCommand>
{
    public async Task Handle(UserActivateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User activation flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (user.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User activation flow skipped for user {UserGuid}; user is already active.",
                    user.Guid);
            }
            return;
        }

        user.Activate();

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.Activated);

        entityEventRepository.Add(EntityEvent.Activated<User>(user, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User activation mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Activate

#region Deactivate

/// <summary>
/// Command used to deactivate a user.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
public sealed record UserDeactivateCommand(Guid UserGuid) : ICommand;

/// <summary>
/// Handles user deactivation.
/// </summary>
/// <remarks>
/// Validates permissions and deactivates the user.
/// </remarks>
public sealed class UserDeactivateCommandHandler(
    IUserRepository userRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserDeactivateCommandHandler> logger) : ICommandHandler<UserDeactivateCommand>
{
    public async Task Handle(UserDeactivateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User deactivation flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (!user.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User deactivation flow skipped for user {UserGuid}; user is already inactive.",
                    user.Guid);
            }
            return;
        }

        user.Deactivate();

        user.MarkAsEditedBy(actor.ActorGuid);

        user.MarkModificationType(UserModifiedType.Deactivated);

        entityEventRepository.Add(EntityEvent.Deactivated<User>(user, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User deactivation mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Deactivate
