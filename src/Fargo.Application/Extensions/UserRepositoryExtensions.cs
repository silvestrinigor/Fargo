using Fargo.Application.Exceptions;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Extensions;

/// <summary>
/// Provides extension methods for retrieving the current authenticated user.
/// </summary>
public static class UserRepositoryExtensions
{
    /// <summary>
    /// Gets the current authenticated user and ensures that the account exists
    /// and is active.
    /// </summary>
    /// <param name="repository">The user repository.</param>
    /// <param name="currentUser">The current user context.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The authenticated and active <see cref="User"/>.</returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be found or when the user is inactive.
    /// </exception>
    public static async Task<User> GetActiveCurrentUser(
        this IUserRepository repository,
        ICurrentUser currentUser,
        CancellationToken cancellationToken = default)
    {
        var actor = await repository.GetByGuid(
            currentUser.UserGuid,
            cancellationToken
            );

        if (actor == null || !actor.IsActive)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        return actor;
    }

    public static async Task<User> GetFoundByGuid(this IUserRepository repository, Guid userGuid, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByGuid(userGuid, cancellationToken)
            ?? throw new UserNotFoundFargoApplicationException(userGuid);

        return user;
    }
}
