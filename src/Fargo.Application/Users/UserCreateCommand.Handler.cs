using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserCreateCommandHandler(
    ActorService actorService, UserService userService,
    IUserRepository userRepository, IPartitionRepository partitionRepository,
    IUserGroupRepository userGroupRepository, ICurrentActor currentActor,
    IPasswordHasher passwordHasher, IUnitOfWork unitOfWork,
    ILogger<UserCreateCommandHandler> logger
) : ICommandHandler<UserCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserCreateCommand command, CancellationToken cancellationToken = default)
    {
        logger.UserCreateStarted(currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.CreateUser);

        var userPasswordHash = passwordHasher.Hash(command.Create.Password);

        await userService.ValidateUserNameidIsAvailableAsync(command.Create.Nameid, cancellationToken);

        var user = User.CreateUser(command.Create.Nameid, userPasswordHash);

        user.FirstName = command.Create.FirstName ?? null;

        user.LastName = command.Create.LastName ?? null;

        user.Description = command.Create.Description ?? Description.Empty;

        user.DefaultPasswordExpirationPeriod = command.Create.DefaultPasswordExpirationTimeSpan ?? null;

        user.MarkPasswordChangeAsRequired();

        if (command.Create.PermissionsToAdd is { Count: > 0 } permissions)
        {
            var requestedActions = permissions.Select(p => p.Action).Distinct().ToHashSet();

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

        logger.UserCreateCompleted(user.Guid, currentActor.ActorId);

        return user.Guid;
    }
}
