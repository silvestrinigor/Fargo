using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.Users.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Operations.Users;

/// <summary>
/// Command used to create a new user.
/// </summary>
public sealed record UserCreateCommand(
    UserCreateDto Create
) : ICommand<Guid>;

public sealed class UserCreateCommandHandler(
    ActorResolver actorService,
    UserService userService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateUser);

        await userService.ValidateUserNameidIsAvailableAsync(command.Create.Nameid, cancellationToken);

        var user = Actor.NewUser(command.Create.Nameid).User!;
        var userActor = user.Actor;

        if (command.Create.FirstName is { } firstName)
        {
            user.FirstName = firstName;
        }

        if (command.Create.LastName is { } lastName)
        {
            user.LastName = lastName;
        }

        if (command.Create.Description is { } description)
        {
            userActor.Description = description;
        }

        if (command.Create.IsActive is true)
        {
            userActor.Activate();
        }
        else if (command.Create.IsActive is false)
        {
            userActor.Deactivate();
        }

        user.Authentication.DefaultPasswordExpirationPeriod = command.Create.Authentication?.DefaultPasswordExpirationPeriod ?? null;

        if (command.Create.Authentication?.Password is { } password)
        {
            var passwordHash = passwordHasher.Hash(password);

            user.Authentication.SetPasswordHash(passwordHash);
        }

        user.Authentication.MarkPasswordChangeAsRequired(timeProvider.GetUtcNow());

        if (command.Create.PermissionsToAdd is { Count: > 0 } permissions)
        {
            var requestedActions = permissions.Distinct();

            foreach (var action in requestedActions)
            {
                actor.ThrowIfPermissionDenied(action);

                userActor.AddPermission(action);
            }
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userActor.AddPartition(partition);
            }
        }

        if (command.Create.UserGroupsToAdd is { Count: > 0 } userGroups)
        {
            foreach (var userGroupGuid in userGroups.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid);

                actor.ThrowIfAccessDenied(userGroup);

                user.AddUserGroup(userGroup);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                userActor.AddPartitionAccess(partition);
            }
        }

        userRepository.Add(user);

        return user.ActorGuid;
    }
}
