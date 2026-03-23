using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.UserGroupCommands;

/// <summary>
/// Command used to create a new <see cref="UserGroup"/>.
/// </summary>
/// <param name="UserGroup">
/// The data required to create the user group.
/// </param>
public sealed record UserGroupCreateCommand(
        UserGroupCreateModel UserGroup
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="UserGroupCreateCommand"/>.
/// </summary>
public sealed class UserGroupCreateCommandHandler(
        ActorService actorService,
        UserGroupService userGroupService,
        IUserGroupRepository userGroupRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserGroupCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new user group.
    /// </summary>
    /// <param name="command">The command containing the user group creation data.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The unique identifier of the created user group.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved.
    /// </exception>
    public async Task<Guid> Handle(
            UserGroupCreateCommand command,
            CancellationToken cancellationToken = default
            )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHassPermission(ActionType.CreateUserGroup);

        var userGroup = new UserGroup
        {
            Nameid = command.UserGroup.Nameid,
            Description = command.UserGroup.Description ?? Description.Empty
        };

        await userGroupService.ValidateUserGroupCreate(userGroup, cancellationToken);

        foreach (var permission in command.UserGroup.Permissions ?? [])
        {
            userGroup.AddPermission(permission.Action);
        }

        userGroupRepository.Add(userGroup);

        await unitOfWork.SaveChanges(cancellationToken);

        return userGroup.Guid;
    }
}
