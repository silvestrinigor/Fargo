using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserCreateCommandHandler(
    ActorService actorService,
    UserService userService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<UserCreateCommandHandler> logger
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User create flow started by actor {actorId}.", currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.EditUser);

        var create = command.Create;

        Nameid nameid;

        Password password;

        try
        {
            nameid = new Nameid(create.Nameid);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }

        try
        {
            password = new Password(create.Password);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }

        var userPasswordHash = passwordHasher.Hash(password);

        var user = User.CreateUser(nameid, userPasswordHash);

        user.MarkPasswordChangeAsRequired();

        await userService.ValidateUserCreate(user, cancellationToken);

        user.FirstName = create.FirstName ?? null;

        user.LastName = create.LastName ?? null;

        user.Description = create.Description ?? Description.Empty;

        user.DefaultPasswordExpirationPeriod = create.DefaultPasswordExpirationTimeSpan ?? null;

        if (create.PermissionsToAdd is { } permissions)
        {
            var requestedActions = permissions.Select(p => p.Action).Distinct().ToHashSet();

            var currentActions = user.Permissions.Select(p => p.Action).ToHashSet();

            if (!requestedActions.SetEquals(currentActions))
            {
                foreach (var action in requestedActions.Except(currentActions))
                {
                    user.AddPermission(action);
                }
            }
        }

        if (create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                user.AddPartition(partition);
            }
        }

        if (create.UserGroupsToAdd is { Count: > 0 } userGroups)
        {
            foreach (var userGroupGuid in userGroups.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuid(userGroupGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(userGroup);

                actor.ThrowIfAccessNotAuthorized(userGroup);

                user.AddUserGroup(userGroup);
            }
        }

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User create mutation completed for user {userGuid} by actor {actorId}.",
                user.Guid,
                actor.ActorId);
        }

        return user.Guid;
    }
}
