using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Security;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Entities;
using Fargo.Core.Shared.Informations;
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
        UserCreateCommand command, CancellationToken cancellationToken = default)
    {
        logger.UserCreateStarted(currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateUser);

        await userService.ValidateUserNameidIsAvailableAsync(command.Create.Nameid, cancellationToken);

        var user = User.CreateUser(command.Create.Nameid);

        user.FirstName = command.Create.FirstName ?? null;

        user.LastName = command.Create.LastName ?? null;

        user.Description = command.Create.Description ?? Description.Empty;

        if (command.Create.IsActive is true)
        {
            user.Activate();
        }
        else if (command.Create.IsActive is false)
        {
            user.Deactivate();
        }

        user.Authentication.DefaultPasswordExpirationPeriod = command.Create.Authentication?.DefaultPasswordExpirationPeriod ?? null;

        if (command.Create.Authentication?.Password is { } password)
        {
            var passwordHash = passwordHasher.Hash(new(command.Create.Authentication.Password));

            user.Authentication.SetPasswordHash(passwordHash);
        }

        user.Authentication.MarkPasswordChangeAsRequired();

        if (command.Create.PermissionsToAdd is { Count: > 0 } permissions)
        {
            var requestedActions = permissions.Distinct();

            foreach (var action in requestedActions)
            {
                actor.ThrowIfPermissionDenied(action);

                user.AddPermission(action);
            }
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.AddPartition(partition);
            }
        }

        if (command.Create.UserGroupsToAdd is { Count: > 0 } userGroups)
        {
            foreach (var userGroupGuid in userGroups.Distinct())
            {
                var userGroup = await userGroupRepository.GetByGuidAsync(userGroupGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, userGroupGuid, EntityType.UserGroup);

                actor.ThrowIfAccessDenied(userGroup);

                user.AddUserGroup(userGroup);
            }
        }

        if (command.Create.PartitionAccessesToAdd is { Count: > 0 } partitionAccessesToAdd)
        {
            foreach (var partitionGuid in partitionAccessesToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                user.AddPartitionAccess(partition);
            }
        }

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UserCreateCompleted(user.Guid, currentActor.Guid);

        return user.Guid;
    }
}
