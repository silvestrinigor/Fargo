using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupUpdateCommandHandler(
    ActorService actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserGroupUpdateCommandHandler> logger
) : ICommandHandler<UserGroupUpdateCommand>
{
    public async Task HandleAsync(
        UserGroupUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var update = command.Update;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group update flow started for user group {UserGroupGuid} by actor {actorId}.",
                command.UserGroupGuid, currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetByGuid(command.UserGroupGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(userGroup);

        actor.ThrowIfAccessNotAuthorized(userGroup);

        if (update.Nameid is not null)
        {
            Nameid nameid;

            try
            {
                nameid = new Nameid(update.Nameid);
            }
            catch (ArgumentException)
            {
                throw new NotImplementedException();
            }

            if (userGroup.Nameid != nameid)
            {
                await userGroupService.ValidateUserGroupNameidChange(userGroup, nameid, cancellationToken);
                userGroup.Nameid = nameid;
            }
        }

        userGroup.Description = update.Description ?? userGroup.Description;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group update mutation completed for user group {UserGroupGuid} by actor {actorId}.",
                userGroup.Guid, actor.ActorId);
        }
    }
}
