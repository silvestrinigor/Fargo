using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Core;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Fargo.Application.Users;

#region DTOs

public sealed record UserDto(
    Guid Guid,
    Nameid Nameid,
    FirstName? FirstName,
    LastName? LastName,
    Description Description,
    TimeSpan? DefaultPasswordExpirationPeriod,
    DateTimeOffset? RequirePasswordChangeAt,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    IReadOnlyCollection<Guid> UserGroups,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record UserCreateDto(
    string Nameid,
    string Password,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    IReadOnlyCollection<Guid>? UserGroups = null
);

public sealed record UserUpdateDto(
    string? Nameid = null,
    FirstName? FirstName = null,
    LastName? LastName = null,
    Description? Description = null,
    string? Password = null,
    bool? IsActive = null,
    IReadOnlyCollection<UserPermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    IReadOnlyCollection<Guid>? UserGroups = null
);

public sealed record UserPermissionUpdateDto(
    ActionType Action
);

public sealed record UserPasswordUpdateDto(
    string NewPassword,
    string? CurrentPassword = null
);

public static class UserDtoMappings
{
    public static readonly Expression<Func<User, UserDto>> Projection = user => new UserDto(
        user.Guid,
        user.Nameid,
        user.FirstName,
        user.LastName,
        user.Description,
        user.DefaultPasswordExpirationPeriod,
        user.RequirePasswordChangeAt,
        user.Permissions.Select(permission => new Permission(permission.Guid, permission.Action)).ToArray(),
        user.Partitions.Select(partition => partition.Guid).ToArray(),
        user.UserGroups.Select(group => group.Guid).ToArray(),
        user.IsActive,
        user.EditedByGuid);
}

#endregion DTOs

#region Exceptions

public class UserNotFoundFargoApplicationException
    : FargoApplicationException
{
    public UserNotFoundFargoApplicationException(Guid userGuid)
        : base($"User with guid '{userGuid}' was not found.")
    {
        UserGuid = userGuid;
    }

    public UserNotFoundFargoApplicationException(Nameid nameid)
        : base($"User with nameid '{nameid}' was not found.")
    {
        Nameid = nameid;
    }

    public Guid? UserGuid { get; }

    public Nameid? Nameid { get; }
}

public sealed class UserNotAuthorizedFargoApplicationException(
    Guid userGuid,
    ActionType actionType
) : FargoApplicationException(
    $"User '{userGuid}' is not authorized to perform action '{actionType}'.")
{
    public Guid UserGuid { get; } = userGuid;

    public ActionType ActionType { get; } = actionType;
}

#endregion Exceptions

#region Repositories

public interface IUserQueryRepository
{
    Task<UserDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}
public static class UserRepositoryExtensions
{
    extension(IUserRepository repository)
    {
        public async Task<User> GetFoundByGuid(
            Guid userGuid,
            CancellationToken cancellationToken = default
        )
        {
            var user = await repository.GetByGuid(userGuid, cancellationToken)
                ?? throw new UserNotFoundFargoApplicationException(userGuid);

            return user;
        }
    }
}

#endregion Repositories

#region Create Delete Update

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

        await userService.ValidateUserCreate(user, cancellationToken);

        userRepository.Add(user);

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

#region Update

/// <summary>
/// Command used to update multiple user properties.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="User">
/// User update data.
/// </param>
public sealed record UserUpdateCommand(
    Guid UserGuid,
    UserUpdateDto User
) : ICommand;

/// <summary>
/// Handles user updates.
/// </summary>
/// <remarks>
/// Validates permissions and applies all specified user changes.
/// </remarks>
public sealed class UserUpdateCommandHandler(
    UserService userService,
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserUpdateCommandHandler> logger
) : ICommandHandler<UserUpdateCommand>
{
    public async Task Handle(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (command.User.Nameid is not null)
        {
            var nameid = ValidateNameid(command.User.Nameid);

            if (user.Nameid != nameid)
            {
                await userService.ValidateUserNameidChange(user, nameid, cancellationToken);
                user.ChangeNameid(nameid);
            }
        }

        if (command.User.FirstName is not null && user.FirstName != command.User.FirstName)
        {
            user.ChangeFirstName(command.User.FirstName);
        }

        if (command.User.LastName is not null && user.LastName != command.User.LastName)
        {
            user.ChangeLastName(command.User.LastName);
        }

        if (command.User.Description is not null && user.Description != command.User.Description)
        {
            user.ChangeDescription(command.User.Description.Value);
        }

        if (command.User.DefaultPasswordExpirationPeriod is not null &&
            user.DefaultPasswordExpirationPeriod != command.User.DefaultPasswordExpirationPeriod.Value)
        {
            user.SetDefaultPasswordExpirationPeriod(command.User.DefaultPasswordExpirationPeriod.Value);
        }

        if (command.User.Password is not null)
        {
            actor.ValidateHasPermission(ActionType.ChangeOtherUserPassword);

            ValidatePasswordPolicy(command.User.Password);

            user.ChangePasswordHash(passwordHasher.Hash(command.User.Password));

            user.MarkPasswordChangeAsRequired();

            var refreshTokens = await refreshTokenRepository.GetByUserGuid(user.Guid, cancellationToken);
            foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
            {
                refreshToken.Revoke();
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "User update flow changed password for user {UserGuid} by actor {ActorGuid}; password change is required.",
                    user.Guid,
                    actor.ActorGuid);
            }
        }

        if (command.User.Permissions is not null)
        {
            UserService.ValidateUserPermissionChange(user, actor.ActorGuid);

            var requestedActions = command.User.Permissions
                .Select(x => x.Action)
                .Distinct()
                .ToHashSet();

            var currentActions = user.Permissions
                .Select(x => x.Action)
                .ToHashSet();

            foreach (var action in requestedActions.Except(currentActions))
            {
                user.AddPermission(action);
            }

            foreach (var action in currentActions.Except(requestedActions))
            {
                user.RemovePermission(action);
            }
        }

        if (command.User.IsActive is not null && user.IsActive != command.User.IsActive.Value)
        {
            if (command.User.IsActive.Value)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }
        }

        #region Partition

        if (command.User.Partitions is { } requestedPartitions)
        {
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

            var partitionsToRemove = user.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                user.RemovePartition(partition);
            }
        }

        #endregion Partition

        #region UserGroup

        if (command.User.UserGroups is { } requestedUserGroups)
        {
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

            var userGroupsToRemove = user.UserGroups
                .Where(g => !requestedUserGroups.Contains(g.Guid))
                .ToList();

            foreach (var userGroup in userGroupsToRemove)
            {
                actor.ValidateHasAccess(userGroup);

                user.RemoveUserGroup(userGroup);
            }
        }

        #endregion UserGroup

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update flow completed for user {UserGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}. PermissionCount: {PermissionCount}. UserGroupCount: {UserGroupCount}.",
                user.Guid,
                actor.ActorGuid,
                user.Partitions.Count,
                user.Permissions.Count,
                user.UserGroups.Count);
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

    private static void ValidatePasswordPolicy(string password)
    {
        try
        {
            _ = new Password(password);
        }
        catch (ArgumentException ex)
        {
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }
    }
}

#endregion Update

#region Focused Updates

/// <summary>
/// Command used to change a user login identifier.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Nameid">
/// New user login identifier.
/// </param>
public sealed record UserChangeNameidCommand(Guid UserGuid, Nameid Nameid) : ICommand;

/// <summary>
/// Handles user login identifier changes.
/// </summary>
/// <remarks>
/// Validates permissions and user nameid uniqueness rules.
/// </remarks>
public sealed class UserChangeNameidCommandHandler(
    UserService userService,
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserChangeNameidCommand>
{
    public async Task Handle(UserChangeNameidCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);

        if (user.Nameid == command.Nameid)
        {
            return;
        }

        await userService.ValidateUserNameidChange(user, command.Nameid, cancellationToken);
        user.ChangeNameid(command.Nameid);
    }
}

/// <summary>
/// Command used to change a user first name.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="FirstName">
/// New user first name.
/// </param>
public sealed record UserChangeFirstNameCommand(Guid UserGuid, FirstName? FirstName) : ICommand;

/// <summary>
/// Handles user first name changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the first name.
/// </remarks>
public sealed class UserChangeFirstNameCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserChangeFirstNameCommand>
{
    public async Task Handle(UserChangeFirstNameCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.ChangeFirstName(command.FirstName);
    }
}

/// <summary>
/// Command used to change a user last name.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="LastName">
/// New user last name.
/// </param>
public sealed record UserChangeLastNameCommand(Guid UserGuid, LastName? LastName) : ICommand;

/// <summary>
/// Handles user last name changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the last name.
/// </remarks>
public sealed class UserChangeLastNameCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserChangeLastNameCommand>
{
    public async Task Handle(UserChangeLastNameCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.ChangeLastName(command.LastName);
    }
}

/// <summary>
/// Command used to change a user description.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="Description">
/// New user description.
/// </param>
public sealed record UserChangeDescriptionCommand(Guid UserGuid, Description Description) : ICommand;

/// <summary>
/// Handles user description changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the description.
/// </remarks>
public sealed class UserChangeDescriptionCommandHandler(
    IUserRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserChangeDescriptionCommand>
{
    public async Task Handle(UserChangeDescriptionCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.ChangeDescription(command.Description);
    }
}

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserSetDefaultPasswordExpirationCommand>
{
    public async Task Handle(UserSetDefaultPasswordExpirationCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.SetDefaultPasswordExpirationPeriod(command.Period);
    }
}

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserChangePasswordCommand>
{
    public async Task Handle(UserChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        actor.ValidateHasPermission(ActionType.ChangeOtherUserPassword);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.ChangePasswordHash(passwordHasher.Hash(command.Password));
        user.MarkPasswordChangeAsRequired();

        var refreshTokens = await refreshTokenRepository.GetByUserGuid(user.Guid, cancellationToken);
        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
        {
            refreshToken.Revoke();
        }
    }
}

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserSetPermissionsCommand>
{
    public async Task Handle(UserSetPermissionsCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        UserService.ValidateUserPermissionChange(user, actor.ActorGuid);

        var requestedActions = command.Actions.Distinct().ToHashSet();
        var currentActions = user.Permissions.Select(x => x.Action).ToHashSet();

        foreach (var action in requestedActions.Except(currentActions))
        {
            user.AddPermission(action);
        }

        foreach (var action in currentActions.Except(requestedActions))
        {
            user.RemovePermission(action);
        }
    }
}

/// <summary>
/// Command used to replace the user partition assignments.
/// </summary>
/// <param name="UserGuid">
/// User unique identifier.
/// </param>
/// <param name="PartitionGuids">
/// Desired partition unique identifiers.
/// </param>
public sealed record UserSetPartitionsCommand(Guid UserGuid, IReadOnlyCollection<Guid> PartitionGuids) : ICommand;

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserSetPartitionsCommand>
{
    public async Task Handle(UserSetPartitionsCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);

        var requestedPartitions = command.PartitionGuids.Distinct().ToArray();

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
    }
}

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserSetUserGroupsCommand>
{
    public async Task Handle(UserSetUserGroupsCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);

        var requestedUserGroups = command.UserGroupGuids.Distinct().ToArray();

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
    }
}

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserActivateCommand>
{
    public async Task Handle(UserActivateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.Activate();
    }
}

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
    ICurrentAuthorizationContext currentAuthorizationContext) : ICommandHandler<UserDeactivateCommand>
{
    public async Task Handle(UserDeactivateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        actor.ValidateHasPermission(ActionType.EditUser);
        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);
        actor.ValidateHasAccess(user);
        user.Deactivate();
    }
}

#endregion Focused Updates

#endregion Create Delete Update

#region Queries

#region Single

public sealed record UserSingleQuery(
    Guid UserGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<UserDto?>;

public sealed class UserSingleQueryHandler(
    IUserQueryRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UserSingleQueryHandler> logger
) : IQueryHandler<UserSingleQuery, UserDto?>
{
    public async Task<UserDto?> Handle(
        UserSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User single query started for user {UserGuid} by actor {ActorGuid}.",
                query.UserGuid,
                actor.ActorGuid);
        }

        var user = await userRepository.GetInfoByGuid(
            query.UserGuid,
            query.AsOfDateTime,
            actor.PartitionAccesses,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User single query completed for user {UserGuid} by actor {ActorGuid}. Found: {Found}.",
                query.UserGuid,
                actor.ActorGuid,
                user is not null);
        }

        return user;
    }
}

#endregion Single

#region Many

public sealed record UsersQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<UserDto>>;

public sealed class UsersQueryHandler(
    IUserQueryRepository userRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<UsersQueryHandler> logger
) : IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(
        UsersQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Users query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actor.ActorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccesses,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var users = await userRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Users query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.ActorGuid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                users.Count);
        }

        return users;
    }
}

#endregion Many

#endregion Queries
