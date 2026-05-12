using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Core;
using Fargo.Core.Partitions;
using Fargo.Core.Tokens;
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

public sealed record UserCreateCommand(
    UserCreateDto User
) : ICommand<Guid>;

public sealed class UserCreateCommandHandler(
    ActorService actorService,
    UserService userService,
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<UserCreateCommandHandler> logger
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        UserCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User create flow started by actor {ActorGuid}.", actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateUser);

        var nameid = ValidateNameid(command.User.Nameid);

        ValidatePasswordPolicy(command.User.Password);

        var userPasswordHash = passwordHasher.Hash(command.User.Password);

        var user = new User
        {
            Nameid = nameid,
            FirstName = command.User.FirstName,
            LastName = command.User.LastName,
            Description = command.User.Description ?? Description.Empty,
            PasswordHash = userPasswordHash
        };

        if (command.User.DefaultPasswordExpirationTimeSpan is not null)
        {
            user.DefaultPasswordExpirationPeriod = command.User.DefaultPasswordExpirationTimeSpan.Value;
        }

        user.MarkPasswordChangeAsRequired();

        #region Partition

        foreach (var partitionGuid in command.User.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            user.Partitions.Add(partition);
        }

        #endregion Partition

        await userService.ValidateUserCreate(user, cancellationToken);

        foreach (var permission in command.User.Permissions ?? [])
        {
            user.AddPermission(permission.Action);
        }

        #region UserGroup

        foreach (var userGroupGuid in command.User.UserGroups ?? [])
        {
            var userGroup = await userGroupRepository.GetFoundByGuid(userGroupGuid, cancellationToken);

            actor.ValidateHasAccess(userGroup);

            if (!userGroup.IsActive)
            {
                throw new UserGroupInactiveFargoDomainException(userGroup.Guid);
            }

            user.UserGroups.Add(userGroup);
        }

        #endregion UserGroup

        userRepository.Add(user);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User create flow completed for user {UserGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}. PermissionCount: {PermissionCount}. UserGroupCount: {UserGroupCount}.",
                user.Guid,
                actor.Guid,
                user.Partitions.Count,
                user.Permissions.Count,
                user.UserGroups.Count);
        }

        return user.Guid;
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

#endregion Create

#region Delete

public sealed record UserDeleteCommand(
    Guid UserGuid
) : ICommand;

public sealed class UserDeleteCommandHandler(
    ActorService actorService,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<UserDeleteCommandHandler> logger
) : ICommandHandler<UserDeleteCommand>
{
    public async Task Handle(
        UserDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        UserService.ValidateUserDelete(user, actor);

        userRepository.Remove(user);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User delete flow completed for user {UserGuid} by actor {ActorGuid}.",
                user.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete

#region Update

public sealed record UserUpdateCommand(
    Guid UserGuid,
    UserUpdateDto User
) : ICommand;

public sealed class UserUpdateCommandHandler(
    ActorService actorService,
    UserService userService,
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<UserUpdateCommandHandler> logger
) : ICommandHandler<UserUpdateCommand>
{
    public async Task Handle(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update flow started for user {UserGuid} by actor {ActorGuid}.",
                command.UserGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (command.User.Nameid is not null)
        {
            var nameid = ValidateNameid(command.User.Nameid);

            if (user.Nameid != nameid)
            {
                await userService.ValidateUserNameidChange(user, nameid, cancellationToken);
                user.Nameid = nameid;
            }
        }

        if (command.User.FirstName is not null && user.FirstName != command.User.FirstName)
        {
            user.FirstName = command.User.FirstName;
        }

        if (command.User.LastName is not null && user.LastName != command.User.LastName)
        {
            user.LastName = command.User.LastName;
        }

        if (command.User.Description is not null && user.Description != command.User.Description)
        {
            user.Description = command.User.Description.Value;
        }

        if (command.User.DefaultPasswordExpirationPeriod is not null &&
            user.DefaultPasswordExpirationPeriod != command.User.DefaultPasswordExpirationPeriod.Value)
        {
            user.DefaultPasswordExpirationPeriod = command.User.DefaultPasswordExpirationPeriod.Value;
        }

        if (command.User.Password is not null)
        {
            actor.ValidateHasPermission(ActionType.ChangeOtherUserPassword);

            ValidatePasswordPolicy(command.User.Password);

            user.PasswordHash = passwordHasher.Hash(command.User.Password);

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
                    actor.Guid);
            }
        }

        if (command.User.Permissions is not null)
        {
            UserService.ValidateUserPermissionChange(user, actor);

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

                user.Partitions.Add(partition);
            }

            var partitionsToRemove = user.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                user.Partitions.Remove(partition);
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

                user.UserGroups.Add(userGroup);
            }

            var userGroupsToRemove = user.UserGroups
                .Where(g => !requestedUserGroups.Contains(g.Guid))
                .ToList();

            foreach (var userGroup in userGroupsToRemove)
            {
                actor.ValidateHasAccess(userGroup);

                user.UserGroups.Remove(userGroup);
            }
        }

        #endregion UserGroup

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User update flow completed for user {UserGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}. PermissionCount: {PermissionCount}. UserGroupCount: {UserGroupCount}.",
                user.Guid,
                actor.Guid,
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

#endregion Create Delete Update

#region Queries

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
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User single query started for user {UserGuid} by actor {ActorGuid}.",
                query.UserGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var user = await userRepository.GetInfoByGuid(
            query.UserGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "User single query completed for user {UserGuid} by actor {ActorGuid}. Found: {Found}.",
                query.UserGuid,
                actor.Guid,
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
        var actorGuid = currentUser.UserGuid;
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Users query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessesGuids,
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
                actor.Guid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                users.Count);
        }

        return users;
    }
}

#endregion Many

#endregion Queries
