using Fargo.Application.Common;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Security;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

/// <summary>
/// Handles the authentication login command by validating the client address,
/// checking authentication attempt limits, validating user credentials, and
/// generating authentication tokens for successfully authenticated users.
/// </summary>
/// <param name="userRepository">Provides access to user data.</param>
/// <param name="passwordHasher">Verifies password hashes against provided passwords.</param>
/// <param name="tokenGenerator">Generates access tokens for authenticated users.</param>
/// <param name="refreshTokenGenerator">Generates refresh tokens for token rotation.</param>
/// <param name="tokenHasher">Hashes refresh tokens before they are persisted.</param>
/// <param name="refreshTokenRepository">Manages refresh token persistence.</param>
/// <param name="attemptService">
/// Tracks authentication attempts and determines whether another authentication
/// attempt is currently allowed.
/// </param>
/// <param name="clientContext">Provides information about the current client request.</param>
/// <param name="unitOfWork">Provides transactional consistency for data operations.</param>
/// <param name="logger">Logs the execution and outcome of the authentication process.</param>
public sealed class IdentityLoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthenticationAttemptService attemptService,
    IClientContext clientContext,
    IUnitOfWork unitOfWork,
    ILogger<IdentityLoginCommandHandler> logger
) : ICommandHandler<IdentityLoginCommand, IdentityAuthResultDto>
{
    /// <summary>
    /// Authenticates a user using the supplied credentials and generates an access
    /// token and refresh token when authentication succeeds.
    /// </summary>
    /// <param name="command">The command containing the user's authentication credentials.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous authentication operation. The task result
    /// contains the generated authentication tokens.
    /// </returns>
    /// <exception cref="AuthenticationRateLimitFargoApplicationException">
    /// Thrown when authentication attempts for the client are currently rate limited.
    /// </exception>
    /// <exception cref="InvalidCredentialsFargoApplicationException">
    /// Thrown when the supplied credentials are invalid or the user cannot authenticate.
    /// </exception>
    /// <exception cref="UserPasswordChangeRequiredFargoApplicationException">
    /// Thrown when the supplied credentials are valid but the user must change
    /// their password before authentication can be completed.
    /// </exception>
    public async Task<IdentityAuthResultDto> HandleAsync(IdentityLoginCommand command, CancellationToken cancellationToken = default)
    {
        logger.IdentityLoginStarted(command.Nameid);

        var shouldLogin = await attemptService.IsAllowedAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

        if (!shouldLogin.Allow)
        {
            logger.IdentityLoginRejectedRateLimit(command.Nameid);

            throw new AuthenticationRateLimitFargoApplicationException(shouldLogin.RetryAfter);
        }

        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Nameid);
        }
        catch (ArgumentException)
        {
            logger.IdentityLoginRejectedInvalidNameId(command.Nameid);

            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var user = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (user is null)
        {
            logger.IdentityLoginRejectedUserNotFound(command.Nameid);

            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (!user.IsActive)
        {
            logger.IdentityLoginRejectedUserNotActive(command.Nameid);

            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException();
        }

        Password password;

        try
        {
            password = new Password(command.Password);
        }
        catch (ArgumentException)
        {
            logger.IdentityLoginRejectedInvalidPasswordFormat(user.Guid);

            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var isValid = user.Authentication.PasswordHash != null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, password);

        if (!isValid)
        {
            logger.IdentityLoginRejectedInvalidPassword(user.Guid);

            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (user.Authentication.IsPasswordChangeRequired)
        {
            logger.IdentityLoginRejectedPasswordChangeRequired(user.Guid);

            await attemptService.RegisterSuccessAsync(
                command.Nameid,
                clientContext.IpAddress,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UserPasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var accessTokenResult = tokenGenerator.Generate(user);

        var rawRefreshToken = refreshTokenGenerator.Generate();

        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.Guid, refreshTokenHash);

        refreshTokenRepository.Add(refreshToken);

        await attemptService.RegisterSuccessAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResult = new IdentityAuthResultDto(
            accessTokenResult.AccessToken.Value, rawRefreshToken.Value, accessTokenResult.ExpiresAt);

        logger.IdentityLoginCompleted(command.Nameid);

        return authResult;
    }
}
