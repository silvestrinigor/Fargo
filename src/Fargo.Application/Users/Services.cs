using Fargo.Application.Identity;
using Fargo.Core;

namespace Fargo.Application.Users;

/// <summary>
/// Provides application operations for users.
/// </summary>
/// <remarks>
/// Coordinates user commands and persists changes
/// using the unit of work.
/// </remarks>
public sealed class UserApplicationService(
    ICommandHandler<UserCreateCommand, Guid> createHandler,
    ICommandHandler<UserChangeNameidCommand> changeNameidHandler,
    ICommandHandler<UserChangeFirstNameCommand> changeFirstNameHandler,
    ICommandHandler<UserChangeLastNameCommand> changeLastNameHandler,
    ICommandHandler<UserChangeDescriptionCommand> changeDescriptionHandler,
    ICommandHandler<UserSetDefaultPasswordExpirationCommand> setDefaultPasswordExpirationHandler,
    ICommandHandler<UserChangePasswordCommand> changePasswordHandler,
    ICommandHandler<UserSetPermissionsCommand> setPermissionsHandler,
    ICommandHandler<UserSetPartitionsCommand> setPartitionsHandler,
    ICommandHandler<UserSetUserGroupsCommand> setUserGroupsHandler,
    ICommandHandler<UserActivateCommand> activateHandler,
    ICommandHandler<UserDeactivateCommand> deactivateHandler,
    ICommandHandler<UserDeleteCommand> deleteHandler,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="create">
    /// User creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The created user identifier.
    /// </returns>
    public async Task<Guid> Create(UserCreateDto create, CancellationToken cancellationToken = default)
    {
        var userGuid = await createHandler.Handle(
            new UserCreateCommand(ToNameid(create.Nameid), ToPassword(create.Password)),
            cancellationToken);

        if (create.FirstName is not null)
        {
            await changeFirstNameHandler.Handle(new UserChangeFirstNameCommand(userGuid, create.FirstName), cancellationToken);
        }

        if (create.LastName is not null)
        {
            await changeLastNameHandler.Handle(new UserChangeLastNameCommand(userGuid, create.LastName), cancellationToken);
        }

        if (create.Description is { } description)
        {
            await changeDescriptionHandler.Handle(new UserChangeDescriptionCommand(userGuid, description), cancellationToken);
        }

        if (create.DefaultPasswordExpirationTimeSpan is { } expirationPeriod)
        {
            await setDefaultPasswordExpirationHandler.Handle(
                new UserSetDefaultPasswordExpirationCommand(userGuid, expirationPeriod),
                cancellationToken);
        }

        if (create.Permissions is { } permissions)
        {
            await setPermissionsHandler.Handle(
                new UserSetPermissionsCommand(userGuid, permissions.Select(p => p.Action).ToArray()),
                cancellationToken);
        }

        if (create.Partitions is { Count: > 0 } partitions)
        {
            await setPartitionsHandler.Handle(new UserSetPartitionsCommand(userGuid, partitions), cancellationToken);
        }

        if (create.UserGroups is { Count: > 0 } userGroups)
        {
            await setUserGroupsHandler.Handle(new UserSetUserGroupsCommand(userGuid, userGroups), cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        return userGuid;
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userGuid">
    /// User unique identifier.
    /// </param>
    /// <param name="update">
    /// User update data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Update(Guid userGuid, UserUpdateDto update, CancellationToken cancellationToken = default)
    {
        if (update.Nameid is not null)
        {
            await changeNameidHandler.Handle(new UserChangeNameidCommand(userGuid, ToNameid(update.Nameid)), cancellationToken);
        }

        if (update.FirstName is not null)
        {
            await changeFirstNameHandler.Handle(new UserChangeFirstNameCommand(userGuid, update.FirstName), cancellationToken);
        }

        if (update.LastName is not null)
        {
            await changeLastNameHandler.Handle(new UserChangeLastNameCommand(userGuid, update.LastName), cancellationToken);
        }

        if (update.Description is { } description)
        {
            await changeDescriptionHandler.Handle(new UserChangeDescriptionCommand(userGuid, description), cancellationToken);
        }

        if (update.DefaultPasswordExpirationPeriod is { } expirationPeriod)
        {
            await setDefaultPasswordExpirationHandler.Handle(
                new UserSetDefaultPasswordExpirationCommand(userGuid, expirationPeriod),
                cancellationToken);
        }

        if (update.Password is not null)
        {
            await changePasswordHandler.Handle(new UserChangePasswordCommand(userGuid, ToPassword(update.Password)), cancellationToken);
        }

        if (update.Permissions is { } permissions)
        {
            await setPermissionsHandler.Handle(
                new UserSetPermissionsCommand(userGuid, permissions.Select(p => p.Action).ToArray()),
                cancellationToken);
        }

        if (update.Partitions is { } partitions)
        {
            await setPartitionsHandler.Handle(new UserSetPartitionsCommand(userGuid, partitions), cancellationToken);
        }

        if (update.UserGroups is { } userGroups)
        {
            await setUserGroupsHandler.Handle(new UserSetUserGroupsCommand(userGuid, userGroups), cancellationToken);
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive)
            {
                await activateHandler.Handle(new UserActivateCommand(userGuid), cancellationToken);
            }
            else
            {
                await deactivateHandler.Handle(new UserDeactivateCommand(userGuid), cancellationToken);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="userGuid">
    /// User unique identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Delete(Guid userGuid, CancellationToken cancellationToken = default)
    {
        await deleteHandler.Handle(new UserDeleteCommand(userGuid), cancellationToken);

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

    private static Password ToPassword(string value)
    {
        try
        {
            return new Password(value);
        }
        catch (ArgumentException ex)
        {
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }
    }
}
