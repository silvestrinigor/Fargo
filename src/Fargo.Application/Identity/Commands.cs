using Fargo.Application.Users;
using Fargo.Core.Shared;
using Fargo.Core.Identity;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

#region Login

/// <summary>
/// Command used to authenticate a user with a nameid and password.
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
public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthorizationContextFactory authorizationContextFactory,
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
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Login flow started.");

        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Nameid);
        }
        catch (ArgumentException)
        {
            logger.LogWarning("Login flow rejected because the provided nameid format is invalid.");
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

        var authorization = await authorizationContextFactory.CreateFromUser(user, cancellationToken);

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
                authorization.IsAdmin,
                authorization.PermissionActions.Count,
                authorization.PartitionAccesses.Count);
        }

        return new AuthResult(
            accessTokenResult.AccessToken,
            rawRefreshToken,
            accessTokenResult.ExpiresAt,
            authorization.IsAdmin,
            authorization.PermissionActions,
            authorization.PartitionAccesses
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
    IAuthorizationContextFactory authorizationContextFactory,
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
        CancellationToken cancellationToken = default)
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
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Refresh flow requires password change for user {UserGuid}; old refresh token was revoked.", user.Guid);
            }
            throw new PasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var authorization = await authorizationContextFactory.CreateFromUser(user, cancellationToken);

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
                authorization.IsAdmin,
                authorization.PermissionActions.Count,
                authorization.PartitionAccesses.Count);
        }

        return new AuthResult(
            newAccessTokenResult.AccessToken,
            rawNewRefreshToken,
            newAccessTokenResult.ExpiresAt,
            authorization.IsAdmin,
            authorization.PermissionActions,
            authorization.PartitionAccesses
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
    public async Task Handle(
        LogoutCommand command,
        CancellationToken cancellationToken = default)
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
        CancellationToken cancellationToken = default)
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

        try
        {
            _ = new Password(command.Passwords.NewPassword);
        }
        catch (ArgumentException ex)
        {
            // TODO: not aways the reason is weak password.
            throw new WeakPasswordFargoApplicationException(ex.Message);
        }

        // TODO: ChangePasswordHash should validate if the actor is the user or the actor has access to change another user password.
        user.ChangePasswordHash(passwordHasher.Hash(command.Passwords.NewPassword));
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
}

#endregion Password Change
