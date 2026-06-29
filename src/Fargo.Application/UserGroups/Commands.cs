using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.UserGroups;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.UserGroups;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.UserGroups;

#region Create

/// <summary>
/// Command used to create a new user group from an API creation payload.
/// </summary>
public sealed record UserGroupCreateCommand(
    UserGroupCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Handles user group creation, including optional create-time state.
/// </summary>
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
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("User group create flow started by actor {actorId}.", currentActor.ActorId);
        }

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
                var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                actor.ThrowIfAccessNotAuthorized(partition);

                userGroup.AddPartition(partition);
            }
        }

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group create mutation completed for user group {userGroupGuid} by actor {actorId}.",
                userGroup.Guid, actor.ActorId);
        }

        return userGroup.Guid;
    }
}

#endregion Create

#region Update

/// <summary>
/// Command used to update an existing user group from an API update payload.
/// </summary>
public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid,
    UserGroupUpdateDto Update
) : ICommand;

/// <summary>
/// Handles user group updates.
/// </summary>
public sealed class UserGroupUpdateCommandHandler(
    ActorService actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
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

#endregion Update

#region Delete

/// <summary>
/// Command used to delete a user group.
/// </summary>
public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;

/// <summary>
/// Handles user group deletion.
/// </summary>
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
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete flow started for user group {userGroupGuid} by actor {actorId}.",
                command.UserGroupGuid,
                currentActor.ActorId);
        }

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        var userGroup = await userGroupRepository.GetByGuid(command.UserGroupGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(userGroup);

        userGroupRepository.Remove(userGroup);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "User group delete mutation completed for user group {userGroupGuid} by actor {actorId}.",
                userGroup.Guid, actor.ActorId);
        }
    }
}

#endregion Delete
