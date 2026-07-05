using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupDeleteCommandHandler(
    ActorService actorService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupDeleteCommandHandler> logger
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task HandleAsync(
        UserGroupDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.DeleteStarted(command.UserGroupGuid, currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(userGroup);

        userGroupRepository.Remove(userGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.DeleteCompleted(command.UserGroupGuid, currentActor.ActorId);
    }
}
