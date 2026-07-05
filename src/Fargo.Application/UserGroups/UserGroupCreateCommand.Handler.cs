using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupCreateCommandHandler(
    ActorService actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupCreateCommandHandler> logger
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.CreateUserGroup);

        Nameid nameid;

        var create = command.Create;

        try
        {
            nameid = new Nameid(create.Nameid);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(ex.Message);
        }

        var userGroup = UserGroup.CreateUserGroup(nameid);

        userGroup.Description = create.Description ?? Description.Empty;

        if (create.PartitionsToAdd is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                actor.ThrowIfAccessNotAuthorized(partition);

                userGroup.AddPartition(partition);
            }
        }

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(userGroup.Guid, currentActor.ActorId);

        return userGroup.Guid;
    }
}
