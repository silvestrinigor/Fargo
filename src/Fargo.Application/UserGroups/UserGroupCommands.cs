using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;

namespace Fargo.Application.UserGroups;

#region Create

public sealed record UserGroupCreateCommand(
    UserGroupCreateDto UserGroup
) : ICommand<Guid>;

public sealed class UserGroupCreateCommandHandler(
    ActorService actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork
) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        UserGroupCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateUserGroup);

        var nameid = ValidateNameid(command.UserGroup.Nameid);

        var userGroup = new UserGroup
        {
            Nameid = nameid,
            Description = command.UserGroup.Description ?? Description.Empty
        };

        #region Partition

        foreach (var partitionGuid in command.UserGroup.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            userGroup.Partitions.Add(partition);
        }

        #endregion Partition

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        foreach (var permission in command.UserGroup.Permissions ?? [])
        {
            userGroup.AddPermission(permission.Action);
        }

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);

        return userGroup.Guid;
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
}

#endregion Create

#region Delete

public sealed record UserGroupDeleteCommand(
    Guid UserGroupGuid
) : ICommand;

public sealed class UserGroupDeleteCommandHandler(
    ActorService actorService,
    IUserGroupRepository userGroupRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task Handle(
        UserGroupDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        UserGroupService.ValidateUserGroupDelete(userGroup, actor);

        userGroupRepository.Remove(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}

#endregion Delete

#region Update

public sealed record UserGroupUpdateCommand(
    Guid UserGroupGuid,
    UserGroupUpdateDto UserGroup
) : ICommand;

public sealed class UserGroupUpdateCommandHandler(
    ActorService actorService,
    IUserGroupRepository userGroupRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<UserGroupUpdateCommand>
{
    public async Task Handle(
        UserGroupUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditUserGroup);

        var userGroup = await userGroupRepository.GetFoundByGuid(command.UserGroupGuid, cancellationToken);

        actor.ValidateHasAccess(userGroup);

        if (command.UserGroup.Nameid is not null)
        {
            var nameid = ValidateNameid(command.UserGroup.Nameid);

            if (userGroup.Nameid != nameid)
            {
                userGroup.Nameid = nameid;
            }
        }

        if (command.UserGroup.Description is not null && userGroup.Description != command.UserGroup.Description)
        {
            userGroup.Description = command.UserGroup.Description.Value;
        }

        if (command.UserGroup.IsActive is not null && userGroup.IsActive != command.UserGroup.IsActive.Value)
        {
            if (command.UserGroup.IsActive.Value)
            {
                userGroup.Activate();
            }
            else
            {
                userGroup.Deactivate();
            }
        }

        if (command.UserGroup.Permissions is not null)
        {
            var requestedActions = command.UserGroup.Permissions
                .Select(x => x.Action)
                .Distinct()
                .ToHashSet();

            var currentActions = userGroup.Permissions
                .Select(x => x.Action)
                .ToHashSet();

            foreach (var action in requestedActions.Except(currentActions))
            {
                userGroup.AddPermission(action);
            }

            foreach (var action in currentActions.Except(requestedActions))
            {
                userGroup.RemovePermission(action);
            }
        }

        #region Partition

        if (command.UserGroup.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (userGroup.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                userGroup.Partitions.Add(partition);
            }

            var partitionsToRemove = userGroup.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                userGroup.Partitions.Remove(partition);
            }
        }

        #endregion Partition

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
}

#endregion Update
