using Fargo.Domain;
using Fargo.Domain.Tokens;
using Fargo.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Authentication;

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
