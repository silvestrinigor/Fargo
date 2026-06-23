using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.Users;
using Fargo.Application.UserGroups;
using Fargo.Core.Events;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

#region Create

/// <summary>
/// Command used to create a new user from an API creation payload.
/// </summary>
public sealed record UserCreateCommand(
    UserCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Handles user creation, including optional create-time state.
/// </summary>
public sealed class UserCreateCommandHandler(
    UserService userService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    IEventRepository entityEventRepository,
    IPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<UserCreateCommandHandler> logger
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var create = command.Create;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User create flow started by actor {ActorGuid}.", actor.Guid);
        }

        Nameid nameid;
        Password password;

        try
        {
            nameid = new Nameid(create.Nameid);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidNameidFargoApplicationException(ex.Message);
        }

        try
        {
            password = new Password(create.Password);
        }
        catch (ArgumentException ex)
        {
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }

        var userPasswordHash = passwordHasher.Hash(password);

        var user = User.CreateUser(nameid, userPasswordHash, actor);
        user.MarkPasswordChangeAsRequired();

        await userService.ValidateUserCreate(user, cancellationToken);

        userRepository.Add(user);

        entityEventRepository.Add(Event.NewEntityCreatedEvent<User>(user, actor.Guid));

        if (create.FirstName is not null)
        {
            user.ChangeFirstName(create.FirstName, actor);
        }

        if (create.LastName is not null)
        {
            user.ChangeLastName(create.LastName, actor);
        }

        if (create.Description is { } description)
        {
            user.ChangeDescription(description, actor);
        }

        if (create.DefaultPasswordExpirationTimeSpan is { } expirationPeriod)
        {
            user.SetDefaultPasswordExpirationPeriod(expirationPeriod, actor);
        }

        if (create.Permissions is { } permissions)
        {
            user.ValidateCanEdit(actor);

            var requestedActions = permissions.Select(p => p.Action).Distinct().ToHashSet();
            var currentActions = user.Permissions.Select(p => p.Action).ToHashSet();

            if (!requestedActions.SetEquals(currentActions))
            {
                UserService.ValidateUserPermissionChange(user, actor);

                foreach (var action in requestedActions.Except(currentActions))
                {
                    user.AddPermission(action, actor);
                }

                foreach (var action in currentActions.Except(requestedActions))
                {
                    user.RemovePermission(action, actor);
                }
            }
        }

        if (create.Partitions is { Count: > 0 } partitions)
        {
            user.ValidateCanEdit(actor);

            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                user.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(PartitionEvent.InsertedIntoPartition(
                    user,
                    partition,
                    actor.Guid));
            }
        }

        if (create.UserGroups is { Count: > 0 } userGroups)
        {
            user.ValidateCanEdit(actor);

            foreach (var userGroupGuid in userGroups.Distinct())
            {
                var userGroup = await userGroupRepository.GetFoundByGuid(userGroupGuid, cancellationToken);

                user.AddUserGroup(userGroup, actor);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User create mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.Guid);
        }

        return user.Guid;
    }
}

#endregion Create

#region Update

/// <summary>
/// Command used to update an existing user from an API update payload.
/// </summary>
public sealed record UserUpdateCommand(
    Guid UserGuid,
    UserUpdateDto Update
) : ICommand;

/// <summary>
/// Handles user updates.
/// </summary>
public sealed class UserUpdateCommandHandler(
    UserService userService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IEventRepository entityEventRepository,
    IPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<UserUpdateCommandHandler> logger
) : ICommandHandler<UserUpdateCommand>
{
    public async Task HandleAsync(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var update = command.Update;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.Guid);
        }

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        user.ValidateCanEdit(actor);

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

            if (user.Nameid != nameid)
            {
                await userService.ValidateUserNameidChange(user, nameid, cancellationToken);
                user.ChangeNameid(nameid, actor);
            }
        }

        if (update.FirstName is not null)
        {
            user.ChangeFirstName(update.FirstName, actor);
        }

        if (update.LastName is not null)
        {
            user.ChangeLastName(update.LastName, actor);
        }

        if (update.Description is { } description)
        {
            user.ChangeDescription(description, actor);
        }

        if (update.DefaultPasswordExpirationPeriod is { } expirationPeriod)
        {
            user.SetDefaultPasswordExpirationPeriod(expirationPeriod, actor);
        }

        if (update.Password is not null)
        {
            Password password;

            try
            {
                password = new Password(update.Password);
            }
            catch (ArgumentException ex)
            {
                throw new WeakPasswordFargoApplicationException(ex.Message);
            }

            user.ChangePasswordHash(passwordHasher.Hash(password), actor);
            user.MarkPasswordChangeAsRequired();

            var refreshTokens = await refreshTokenRepository.GetByUserGuid(user.Guid, cancellationToken);

            foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
            {
                refreshToken.Revoke();
            }
        }

        if (update.Permissions is { } permissions)
        {
            var requestedActions = permissions.Select(p => p.Action).Distinct().ToHashSet();
            var currentActions = user.Permissions.Select(p => p.Action).ToHashSet();

            if (!requestedActions.SetEquals(currentActions))
            {
                UserService.ValidateUserPermissionChange(user, actor);

                foreach (var action in requestedActions.Except(currentActions))
                {
                    user.AddPermission(action, actor);
                }

                foreach (var action in currentActions.Except(requestedActions))
                {
                    user.RemovePermission(action, actor);
                }
            }
        }

        if (update.Partitions is { } partitions)
        {
            var requestedPartitionGuids = partitions.Distinct().ToArray();

            foreach (var partitionGuid in requestedPartitionGuids)
            {
                if (user.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                user.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(PartitionEvent.InsertedIntoPartition(
                    user,
                    partition,
                    actor.Guid));
            }

            var partitionsToRemove = user.Partitions
                .Where(p => !requestedPartitionGuids.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                user.RemovePartition(partition, actor);

                entityPartitionEventRepository.Add(PartitionEvent.RemovedFromPartition(
                    user,
                    partition,
                    actor.Guid));
            }
        }

        if (update.UserGroups is { } userGroups)
        {
            var requestedUserGroupGuids = userGroups.Distinct().ToArray();

            foreach (var userGroupGuid in requestedUserGroupGuids)
            {
                if (user.UserGroups.Any(g => g.Guid == userGroupGuid))
                {
                    continue;
                }

                var userGroup = await userGroupRepository.GetFoundByGuid(userGroupGuid, cancellationToken);

                user.AddUserGroup(userGroup, actor);
            }

            var userGroupsToRemove = user.UserGroups
                .Where(g => !requestedUserGroupGuids.Contains(g.Guid))
                .ToList();

            foreach (var userGroup in userGroupsToRemove)
            {
                user.RemoveUserGroup(userGroup, actor);
            }
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive && !user.IsActive)
            {
                user.Activate(actor);
                entityEventRepository.Add(Event.Activated<User>(user, actor.Guid));
            }
            else if (!isActive && user.IsActive)
            {
                user.Deactivate(actor);
                entityEventRepository.Add(Event.Deactivated<User>(user, actor.Guid));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.Guid);
        }
    }
}

#endregion Update

#region Delete

/// <summary>
/// Command used to delete a user.
/// </summary>
public sealed record UserDeleteCommand(
    Guid UserGuid
) : ICommand;

/// <summary>
/// Handles user deletion.
/// </summary>
public sealed class UserDeleteCommandHandler(
    IUserRepository userRepository,
    IEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<UserDeleteCommandHandler> logger
) : ICommandHandler<UserDeleteCommand>
{
    public async Task HandleAsync(
        UserDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actor.Guid);
        }

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        UserService.ValidateUserDelete(user, actor);

        userRepository.Remove(user);

        entityEventRepository.Add(Event.EntityDeleted<User>(user, actor.Guid));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete mutation completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete
