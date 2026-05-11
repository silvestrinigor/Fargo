using Fargo.Application.Partitions;
using Fargo.Application.Users;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Tokens;
using Fargo.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Authentication;

#region Contracts

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
/// <remarks>
/// This abstraction allows the application layer to access
/// user identity information without depending on HTTP,
/// authentication frameworks, or infrastructure concerns.
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Guid.Empty"/> when the user is not authenticated.
    /// </remarks>
    Guid UserGuid
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated
    {
        get;
    }
}
/// <summary>
/// Defines a service responsible for generating access tokens
/// for authenticated users.
/// </summary>
public interface ITokenGenerator
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">
    /// The user for whom the token will be generated.
    /// </param>
    /// <returns>
    /// A <see cref="TokenGenerateResult"/> containing the generated
    /// access token and its expiration information.
    /// </returns>
    TokenGenerateResult Generate(User user);
}
/// <summary>
/// Represents the result of generating an access token.
/// </summary>
/// <param name="AccessToken">
/// The generated access token used to authenticate requests.
/// </param>
/// <param name="ExpiresAt">
/// The date and time when the access token expires.
/// </param>
public record TokenGenerateResult(
        Token AccessToken,
        DateTimeOffset ExpiresAt
        );
/// <summary>
/// Represents the result of a successful authentication.
/// </summary>
/// <param name="AccessToken">
/// The generated access token used to authenticate API requests.
/// </param>
/// <param name="RefreshToken">
/// The refresh token used to obtain a new access token when the current one expires.
/// </param>
/// <param name="ExpiresAt">
/// The date and time when the access token expires.
/// </param>
/// <param name="IsAdmin">
/// Whether the authenticated user has administrator privileges.
/// </param>
/// <param name="PermissionActions">
/// The effective set of actions the user is permitted to perform,
/// combining direct permissions and those inherited from user groups.
/// </param>
/// <param name="PartitionAccesses">
/// The effective set of partition identifiers the user can access,
/// including inherited group accesses and all descendant partitions.
/// </param>
public record AuthResult(
        Token AccessToken,
        Token RefreshToken,
        DateTimeOffset ExpiresAt,
        bool IsAdmin,
        IReadOnlyCollection<ActionType> PermissionActions,
        IReadOnlyCollection<Guid> PartitionAccesses
        );
/// <summary>
/// Configuration options used to create the default administrator
/// account during system initialization.
/// </summary>
/// <remarks>
/// These settings are used by the system initialization process to
/// create a default administrator user when no users exist in the system.
///
/// The values are typically provided through application configuration
/// (for example <c>appsettings.json</c> or environment variables).
///
/// This configuration is only used during system initialization and
/// does not affect authentication after the administrator account
/// has been created.
/// </remarks>
public sealed class DefaultAdminOptions
{
    /// <summary>
    /// The configuration section name used for binding these options.
    /// </summary>
    public const string SectionName = "DefaultAdmin";

    /// <summary>
    /// Gets the identifier (NAMEID) of the default administrator account.
    /// </summary>
    /// <remarks>
    /// This value defines the unique login identifier that will be assigned
    /// to the administrator user created during system initialization.
    /// </remarks>
    public required string Nameid { get; init; }

    /// <summary>
    /// Gets the password of the default administrator account.
    /// </summary>
    /// <remarks>
    /// This password is used only when the administrator account is
    /// automatically created during system initialization.
    ///
    /// It should be replaced or changed immediately after the system
    /// is first deployed to ensure proper security.
    /// </remarks>
    public required string Password { get; init; }
}

#endregion Contracts

#region Exceptions

/// <summary>
/// Exception thrown when login fails due to invalid credentials.
/// </summary>
/// <remarks>
/// Used when the user cannot be authenticated — whether because the user does not exist,
/// the password is incorrect, or the account is inactive. A single exception type is
/// intentionally used for all these cases to avoid leaking information about which
/// condition was triggered.
/// </remarks>
public sealed class InvalidCredentialsFargoApplicationException()
    : FargoApplicationException("The provided credentials are invalid.");
/// <summary>
/// Exception thrown when a nameid string does not satisfy the required format rules.
/// </summary>
/// <param name="reason">A message describing the specific rule violation.</param>
public sealed class InvalidNameidFargoApplicationException(string reason)
    : FargoApplicationException(reason);
/// <summary>
/// Exception thrown when the provided password does not match
/// the current password of the user.
/// </summary>
/// <remarks>
/// This exception should only be used when the user is already authenticated
/// and their identity is known.
///
/// Returning a specific "invalid password" error for unauthenticated requests
/// can expose security information by allowing attackers to distinguish
/// between an invalid user identifier and an incorrect password.
///
/// For authentication failures where the user identity is not yet verified,
/// a generic authentication error (for example
/// <see cref="UnauthorizedAccessFargoApplicationException"/>) should be used instead.
/// </remarks>
public class InvalidPasswordFargoApplicationException()
    : FargoApplicationException("The provided password is incorrect.");
/// <summary>
/// Exception thrown when a user must change their password before accessing the system.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PasswordChangeRequiredFargoApplicationException"/> class.
/// </remarks>
/// <param name="userGuid">
/// The identifier of the user who must change their password.
/// </param>
public sealed class PasswordChangeRequiredFargoApplicationException(Guid userGuid)
            : FargoApplicationException($"User '{userGuid}' must change their password before continuing.")
{
    /// <summary>
    /// Gets the identifier of the user who must change their password.
    /// </summary>
    public Guid UserGuid { get; } = userGuid;
}
/// <summary>
/// Exception thrown when the current user is not authorized
/// to perform the requested operation.
/// </summary>
public class UnauthorizedAccessFargoApplicationException()
    : FargoApplicationException(
            "The current user is not authorized to perform this operation.");
/// <summary>
/// Exception thrown when a new password does not meet the required security policy.
/// </summary>
/// <remarks>
/// This exception is only thrown when setting or changing a password, never
/// during login or current-password verification.
/// </remarks>
/// <param name="reason">A message describing the specific policy violation.</param>
public sealed class WeakPasswordFargoApplicationException(string reason)
    : FargoApplicationException(reason);

#endregion Exceptions

#region Actor Helpers

/// <summary>
/// Provides extension methods for <see cref="Actor"/> to enforce
/// authorization and partition-based access control rules.
/// </summary>
/// <remarks>
/// These methods centralize validation logic used across the application layer,
/// ensuring that authorization checks are consistently applied before executing
/// domain operations.
/// </remarks>
public static class ActorExtensions
{
    extension(Actor actor)
    {
        /// <summary>
        /// Ensures that the actor has permission to execute the specified action.
        /// </summary>
        /// <param name="action">
        /// The action that requires authorization.
        /// </param>
        /// <exception cref="UserNotAuthorizedFargoApplicationException">
        /// Thrown when the actor does not have permission to perform the specified action.
        /// </exception>
        /// <remarks>
        /// This validation enforces action-level authorization rules defined by the domain.
        /// </remarks>
        public void ValidateHasPermission(ActionType action)
        {
            if (!actor.HasActionPermission(action))
            {
                throw new UserNotAuthorizedFargoApplicationException(actor.Guid, action);
            }
        }

        /// <summary>
        /// Ensures that the actor has access to the specified partition.
        /// </summary>
        /// <param name="partitionGuid">
        /// The unique identifier of the partition to validate access for.
        /// </param>
        /// <exception cref="PartitionAccessDeniedFargoApplicationException">
        /// Thrown when the actor does not have access to the specified partition.
        /// </exception>
        /// <remarks>
        /// This validation enforces partition-level isolation, ensuring that actors
        /// can only interact with data within their authorized partitions.
        /// </remarks>
        public void ValidateHasPartitionAccess(Guid partitionGuid)
        {
            if (!actor.HasPartitionAccess(partitionGuid))
            {
                throw new PartitionAccessDeniedFargoApplicationException(partitionGuid, actor.Guid);
            }
        }

        /// <summary>
        /// Ensures that the actor has access to the specified partitioned entity.
        /// </summary>
        /// <typeparam name="TEntity">
        /// The type of the partitioned entity.
        /// </typeparam>
        /// <param name="partitioned">
        /// The entity whose access should be validated.
        /// </param>
        /// <exception cref="PartitionedEntityAccessDeniedFargoApplicationException">
        /// Thrown when the actor does not have access to the specified entity.
        /// </exception>
        /// <remarks>
        /// This validation combines entity-level and partition-level checks,
        /// ensuring that the actor can access the given entity based on its
        /// partition associations.
        /// </remarks>
        public void ValidateHasAccess<TEntity>(TEntity partitioned)
            where TEntity : IEntity, IPartitionedEntity
        {
            if (!actor.HasAccess(partitioned))
            {
                throw new PartitionedEntityAccessDeniedFargoApplicationException(partitioned.Guid, actor.Guid);
            }
        }
    }
}
/// <summary>
/// Provides authorization-related extension methods for <see cref="ActorService"/>.
/// </summary>
/// <remarks>
/// These extensions centralize common validation logic for retrieving actors,
/// ensuring they exist and are in a valid state before being used in application operations.
/// </remarks>
public static class ActorServiceExtensions
{
    extension(ActorService service)
    {
        /// <summary>
        /// Retrieves an <see cref="Actor"/> by its GUID and ensures it is valid and active.
        /// </summary>
        /// <param name="actorGuid">
        /// The unique identifier of the actor.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A valid and active <see cref="Actor"/> instance.
        /// </returns>
        /// <exception cref="UnauthorizedAccessFargoApplicationException">
        /// Thrown when the actor does not exist or is not active.
        /// </exception>
        /// <remarks>
        /// This method is typically used at the beginning of application workflows
        /// to ensure that the current actor is authenticated and eligible to perform
        /// further operations.
        /// </remarks>
        public async Task<Actor> GetAuthorizedActorByGuid(
            Guid actorGuid,
            CancellationToken cancellationToken = default
        )
        {
            var actor = await service.GetActorByGuid(actorGuid, cancellationToken);

            if (actor is null || !actor.IsActive)
            {
                throw new UnauthorizedAccessFargoApplicationException();
            }

            return actor;
        }
    }
}

#endregion Actor Helpers

#region Login

/// <summary>
/// Command used to authenticate a user with a NAMEID and password.
/// </summary>
/// <param name="Nameid">
/// The unique user identifier used for login.
/// </param>
/// <param name="Password">
/// The plaintext password provided for authentication.
/// </param>
public sealed record LoginCommand(
        string Nameid,
        string Password
        ) : ICommand<AuthResult>;

/// <summary>
/// Handles the execution of <see cref="LoginCommand"/>.
/// </summary>
/// <remarks>
/// This handler validates the provided credentials, checks whether the
/// user is allowed to sign in, and issues new access and refresh tokens.
/// </remarks>
public sealed class LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenHasher tokenHasher,
        IRefreshTokenRepository refreshTokenRepository,
        ActorService actorService,
        IUnitOfWork unitOfWork,
        ILogger<LoginCommandHandler> logger
        ) : ICommandHandler<LoginCommand, AuthResult>
{
    /// <summary>
    /// Authenticates a user and generates access and refresh tokens.
    /// </summary>
    /// <param name="command">
    /// The command containing the login credentials.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the generated access token,
    /// refresh token, and access token expiration time.
    /// </returns>
    /// <exception cref="InvalidCredentialsFargoApplicationException">
    /// Thrown when the user does not exist, the password is invalid,
    /// or the user is inactive.
    /// </exception>
    /// <exception cref="PasswordChangeRequiredFargoApplicationException">
    /// Thrown when the user must change their password before continuing.
    /// </exception>
    public async Task<AuthResult> Handle(
            LoginCommand command,
            CancellationToken cancellationToken = default
            )
    {
        logger.LogInformation("Login flow started.");

        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Nameid);
        }
        catch (ArgumentException)
        {
            logger.LogWarning("Login flow rejected because the provided NAMEID format is invalid.");
            throw new InvalidCredentialsFargoApplicationException();
        }

        var user = await userRepository.GetByNameid(
                nameid,
                cancellationToken
                );

        if (user is null)
        {
            logger.LogWarning("Login flow rejected because the user was not found.");
            throw new InvalidCredentialsFargoApplicationException();
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Login flow rejected for inactive user {UserGuid}.", user.Guid);
            throw new InvalidCredentialsFargoApplicationException();
        }

        var isValid = passwordHasher.Verify(
                user.PasswordHash,
                command.Password
                );

        if (!isValid)
        {
            logger.LogWarning("Login flow rejected because the password was invalid for user {UserGuid}.", user.Guid);
            throw new InvalidCredentialsFargoApplicationException();
        }

        if (user.IsPasswordChangeRequired)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Login flow requires password change for user {UserGuid}.", user.Guid);
            }
            throw new PasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var actor = (UserActor)(await actorService.GetActorByGuid(user.Guid, cancellationToken))!;

        var accessTokenResult = tokenGenerator.Generate(user);

        var rawRefreshToken = refreshTokenGenerator.Generate();

        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = refreshTokenHash
        };

        refreshTokenRepository.Add(refreshToken);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Login flow completed for user {UserGuid}. IsAdmin: {IsAdmin}. PermissionCount: {PermissionCount}. PartitionAccessCount: {PartitionAccessCount}.",
                user.Guid,
                actor.IsAdmin,
                actor.PermissionActions.Count,
                actor.PartitionAccessesGuids.Count);
        }

        return new AuthResult(
                accessTokenResult.AccessToken,
                rawRefreshToken,
                accessTokenResult.ExpiresAt,
                actor.IsAdmin,
                actor.PermissionActions,
                actor.PartitionAccessesGuids
                );
    }
}

#endregion Login

#region Refresh

/// <summary>
/// Command used to refresh authentication tokens using a valid refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record RefreshCommand(
        Token RefreshToken
        ) : ICommand<AuthResult>;

/// <summary>
/// Handles the execution of <see cref="RefreshCommand"/>.
/// </summary>
/// <remarks>
/// This handler validates the provided refresh token, resolves the
/// associated user, rotates the refresh token, and generates a new
/// access token.
/// </remarks>
public sealed class RefreshCommandHandler(
        IUserRepository userRepository,
        ITokenGenerator tokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenHasher tokenHasher,
        IRefreshTokenRepository refreshTokenRepository,
        ActorService actorService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshCommandHandler> logger
        ) : ICommandHandler<RefreshCommand, AuthResult>
{
    /// <summary>
    /// Validates the provided refresh token, rotates it, and generates a new access token.
    /// </summary>
    /// <param name="command">
    /// The command containing the refresh token.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// An <see cref="AuthResult"/> containing the new access token,
    /// new refresh token, and access token expiration time.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the refresh token is invalid, expired, the user cannot
    /// be resolved, or the user is inactive.
    /// </exception>
    public async Task<AuthResult> Handle(
            RefreshCommand command,
            CancellationToken cancellationToken = default
            )
    {
        logger.LogInformation("Refresh flow started.");

        var oldRefreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedOldRefreshToken = await refreshTokenRepository.GetByTokenHash(
                oldRefreshTokenHash,
                cancellationToken
                );

        if (storedOldRefreshToken == null || !storedOldRefreshToken.IsUsable)
        {
            logger.LogWarning("Refresh flow rejected because the refresh token was missing or not usable.");
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var user = await userRepository.GetByGuid(
                storedOldRefreshToken.UserGuid,
                cancellationToken
                );

        if (user is null)
        {
            logger.LogWarning(
                "Refresh flow rejected because user {UserGuid} from the refresh token was not found.",
                storedOldRefreshToken.UserGuid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (!user.IsActive)
        {
            storedOldRefreshToken.Revoke();
            await unitOfWork.SaveChanges(cancellationToken);
            logger.LogWarning("Refresh flow rejected for inactive user {UserGuid}; old refresh token was revoked.", user.Guid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (user.IsPasswordChangeRequired)
        {
            storedOldRefreshToken.Revoke();
            await unitOfWork.SaveChanges(cancellationToken);
            logger.LogInformation("Refresh flow requires password change for user {UserGuid}; old refresh token was revoked.", user.Guid);
            throw new PasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var actor = (UserActor)(await actorService.GetActorByGuid(user.Guid, cancellationToken))!;

        var rawNewRefreshToken = refreshTokenGenerator.Generate();

        var newRefreshTokenHash = tokenHasher.Hash(rawNewRefreshToken);

        var storedNewRefreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = newRefreshTokenHash
        };

        storedOldRefreshToken.ReplaceWith(newRefreshTokenHash);

        refreshTokenRepository.Add(storedNewRefreshToken);

        var newAccessTokenResult = tokenGenerator.Generate(user);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Refresh flow completed for user {UserGuid}. IsAdmin: {IsAdmin}. PermissionCount: {PermissionCount}. PartitionAccessCount: {PartitionAccessCount}.",
                user.Guid,
                actor.IsAdmin,
                actor.PermissionActions.Count,
                actor.PartitionAccessesGuids.Count);
        }

        return new AuthResult(
                newAccessTokenResult.AccessToken,
                rawNewRefreshToken,
                newAccessTokenResult.ExpiresAt,
                actor.IsAdmin,
                actor.PermissionActions,
                actor.PartitionAccessesGuids
                );
    }
}

#endregion Refresh

#region Logout

/// <summary>
/// Command used to log out a user by invalidating a refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh token provided by the client.
/// </param>
public sealed record LogoutCommand(
        Token RefreshToken
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="LogoutCommand"/>.
/// </summary>
/// <remarks>
/// This handler invalidates the provided refresh token if it exists.
/// If the token is not found, the operation completes silently.
/// </remarks>
public sealed class LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        ILogger<LogoutCommandHandler> logger
        ) : ICommandHandler<LogoutCommand>
{
    /// <summary>
    /// Invalidates the provided refresh token.
    /// </summary>
    /// <param name="command">
    /// The command containing the refresh token.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous logout operation.
    /// </returns>
    /// <remarks>
    /// If the refresh token does not exist in storage, the method returns
    /// without throwing an exception.
    /// </remarks>
    public async Task Handle(
            LogoutCommand command,
            CancellationToken cancellationToken = default
            )
    {
        logger.LogInformation("Logout flow started.");

        var refreshTokenHash = tokenHasher.Hash(command.RefreshToken);

        var storedRefreshToken = await refreshTokenRepository.GetByTokenHash(
                refreshTokenHash,
                cancellationToken
                );

        if (storedRefreshToken == null)
        {
            logger.LogWarning("Logout flow completed without revoking a refresh token because it was not found.");
            return;
        }

        storedRefreshToken.Revoke();

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Logout flow completed for user {UserGuid}.", storedRefreshToken.UserGuid);
        }
    }
}

#endregion Logout

#region Password Change

/// <summary>
/// Command used by an authenticated user to change their own password.
/// </summary>
/// <param name="Passwords">
/// The current password and the new password.
/// </param>
public sealed record PasswordChangeCommand(
        UserPasswordUpdateDto Passwords
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="PasswordChangeCommand"/>.
/// </summary>
/// <remarks>
/// This handler validates the current authenticated user, verifies the
/// provided current password, updates the password hash, and resets the
/// password expiration date.
/// </remarks>
public sealed class PasswordChangeCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<PasswordChangeCommandHandler> logger
        ) : ICommandHandler<PasswordChangeCommand>
{
    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <param name="command">
    /// The command containing the current password and the new password.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved or the user is inactive.
    /// </exception>
    /// <exception cref="InvalidPasswordFargoApplicationException">
    /// Thrown when the current password is invalid.
    /// </exception>
    public async Task Handle(
            PasswordChangeCommand command,
            CancellationToken cancellationToken = default
    )
    {
        var userGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Password change flow started for user {UserGuid}.", userGuid);
        }

        var user = await userRepository.GetByGuid(
                userGuid,
                cancellationToken
                );

        if (user is null)
        {
            logger.LogWarning("Password change flow rejected because user {UserGuid} was not found.", userGuid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Password change flow rejected for inactive user {UserGuid}.", user.Guid);
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (command.Passwords.CurrentPassword is null)
        {
            logger.LogWarning(
                "Password change flow rejected because the current password was missing for user {UserGuid}.",
                user.Guid);
            throw new InvalidPasswordFargoApplicationException();
        }

        var currentPassword = command.Passwords.CurrentPassword;

        var isValid = passwordHasher.Verify(
                user.PasswordHash,
                currentPassword
                );

        if (!isValid)
        {
            logger.LogWarning("Password change flow rejected because the current password was invalid for user {UserGuid}.", user.Guid);
            throw new InvalidPasswordFargoApplicationException();
        }

        ValidatePasswordPolicy(command.Passwords.NewPassword);

        user.PasswordHash = passwordHasher.Hash(command.Passwords.NewPassword);
        user.ResetPasswordExpiration();
        user.RotateAuthVersion();

        var refreshTokens = await refreshTokenRepository.GetByUserGuid(user.Guid, cancellationToken);
        foreach (var refreshToken in refreshTokens.Where(refreshToken => refreshToken.IsUsable))
        {
            refreshToken.Revoke();
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Password change flow completed for user {UserGuid}.", user.Guid);
        }
    }

    private static void ValidatePasswordPolicy(string password)
    {
        try
        {
            _ = new Password(password);
        }
        catch (ArgumentException ex)
        {
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }
    }
}

#endregion Password Change
