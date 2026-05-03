using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.Application.Users;

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
    IUnitOfWork unitOfWork
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        UserCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

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

            user.UserGroups.Add(userGroup);
        }

        #endregion UserGroup

        userRepository.Add(user);

        await unitOfWork.SaveChanges(cancellationToken);

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
    ICurrentUser currentUser
) : ICommandHandler<UserDeleteCommand>
{
    public async Task Handle(
        UserDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        UserService.ValidateUserDelete(user, actor);

        userRepository.Remove(user);

        await unitOfWork.SaveChanges(cancellationToken);
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
    IUserRepository userRepository,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<UserUpdateCommand>
{
    public async Task Handle(
        UserUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUser);

        var user = await userRepository.GetFoundByGuid(command.UserGuid, cancellationToken);

        actor.ValidateHasAccess(user);

        if (command.User.Nameid is not null)
        {
            user.Nameid = ValidateNameid(command.User.Nameid);
        }

        user.FirstName = command.User.FirstName ?? user.FirstName;
        user.LastName = command.User.LastName ?? user.LastName;
        user.Description = command.User.Description ?? user.Description;

        if (command.User.DefaultPasswordExpirationPeriod is not null)
        {
            user.DefaultPasswordExpirationPeriod = command.User.DefaultPasswordExpirationPeriod.Value;
        }

        if (command.User.Password is not null)
        {
            actor.ValidateHasPermission(ActionType.ChangeOtherUserPassword);

            ValidatePasswordPolicy(command.User.Password);

            user.PasswordHash = passwordHasher.Hash(command.User.Password);

            user.MarkPasswordChangeAsRequired();
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

        if (command.User.IsActive is not null)
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

        foreach (var partitionGuid in command.User.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            user.Partitions.Add(partition);
        }

        #endregion Partition

        #region UserGroup

        foreach (var userGroupGuid in command.User.UserGroups ?? [])
        {
            var userGroup = await userGroupRepository.GetFoundByGuid(userGroupGuid, cancellationToken);

            actor.ValidateHasAccess(userGroup);

            user.UserGroups.Add(userGroup);
        }

        #endregion UserGroup

        await unitOfWork.SaveChanges(cancellationToken);
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
