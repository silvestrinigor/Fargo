using Fargo.Application.Identity;
using Fargo.Core;

namespace Fargo.Application.UserGroups;

/// <summary>
/// Provides application operations for user groups.
/// </summary>
/// <remarks>
/// Coordinates user group commands and persists changes
/// using the unit of work.
/// </remarks>
public sealed class UserGroupApplicationService(
    ICommandHandler<UserGroupCreateCommand, Guid> createHandler,
    ICommandHandler<UserGroupChangeNameidCommand> changeNameidHandler,
    ICommandHandler<UserGroupChangeDescriptionCommand> changeDescriptionHandler,
    ICommandHandler<UserGroupSetPermissionsCommand> setPermissionsHandler,
    ICommandHandler<UserGroupSetPartitionsCommand> setPartitionsHandler,
    ICommandHandler<UserGroupActivateCommand> activateHandler,
    ICommandHandler<UserGroupDeactivateCommand> deactivateHandler,
    ICommandHandler<UserGroupDeleteCommand> deleteHandler,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Creates a new user group.
    /// </summary>
    /// <param name="create">
    /// User group creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The created user group identifier.
    /// </returns>
    public async Task<Guid> Create(UserGroupCreateDto create, CancellationToken cancellationToken = default)
    {
        var userGroupGuid = await createHandler.Handle(
            new UserGroupCreateCommand(ToNameid(create.Nameid)),
            cancellationToken);

        if (create.Description is { } description)
        {
            await changeDescriptionHandler.Handle(
                new UserGroupChangeDescriptionCommand(userGroupGuid, description),
                cancellationToken);
        }

        if (create.Permissions is { } permissions)
        {
            await setPermissionsHandler.Handle(
                new UserGroupSetPermissionsCommand(userGroupGuid, permissions.Select(p => p.Action).ToArray()),
                cancellationToken);
        }

        if (create.Partitions is { Count: > 0 } partitions)
        {
            await setPartitionsHandler.Handle(
                new UserGroupSetPartitionsCommand(userGroupGuid, partitions),
                cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        return userGroupGuid;
    }

    /// <summary>
    /// Updates an existing user group.
    /// </summary>
    /// <param name="userGroupGuid">
    /// User group unique identifier.
    /// </param>
    /// <param name="update">
    /// User group update data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Update(Guid userGroupGuid, UserGroupUpdateDto update, CancellationToken cancellationToken = default)
    {
        if (update.Nameid is not null)
        {
            await changeNameidHandler.Handle(
                new UserGroupChangeNameidCommand(userGroupGuid, ToNameid(update.Nameid)),
                cancellationToken);
        }

        if (update.Description is { } description)
        {
            await changeDescriptionHandler.Handle(
                new UserGroupChangeDescriptionCommand(userGroupGuid, description),
                cancellationToken);
        }

        if (update.Permissions is { } permissions)
        {
            await setPermissionsHandler.Handle(
                new UserGroupSetPermissionsCommand(userGroupGuid, permissions.Select(p => p.Action).ToArray()),
                cancellationToken);
        }

        if (update.Partitions is { } partitions)
        {
            await setPartitionsHandler.Handle(
                new UserGroupSetPartitionsCommand(userGroupGuid, partitions),
                cancellationToken);
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive)
            {
                await activateHandler.Handle(new UserGroupActivateCommand(userGroupGuid), cancellationToken);
            }
            else
            {
                await deactivateHandler.Handle(new UserGroupDeactivateCommand(userGroupGuid), cancellationToken);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }

    /// <summary>
    /// Deletes a user group.
    /// </summary>
    /// <param name="userGroupGuid">
    /// User group unique identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Delete(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        await deleteHandler.Handle(new UserGroupDeleteCommand(userGroupGuid), cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }

    private static Nameid ToNameid(string value)
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
