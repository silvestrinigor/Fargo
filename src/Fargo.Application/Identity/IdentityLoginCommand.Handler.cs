using Fargo.Application.Common;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Security;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

/// <summary>
/// Handles the authentication login command by validating user credentials and generating authentication tokens.
/// </summary>
/// <param name="userRepository">Provides access to user data.</param>
/// <param name="passwordHasher">Verifies password hashes against provided passwords.</param>
/// <param name="tokenGenerator">Generates access tokens for authenticated users.</param>
/// <param name="refreshTokenGenerator">Generates refresh tokens for token rotation.</param>
/// <param name="tokenHasher">Hashes refresh tokens for secure storage.</param>
/// <param name="refreshTokenRepository">Manages refresh token persistence.</param>
/// <param name="unitOfWork">Provides transactional consistency for data operations.</param>
/// <param name="logger">Logs the execution of the authentication process.</param>
public sealed class IdentityLoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<IdentityLoginCommandHandler> logger
) : ICommandHandler<IdentityLoginCommand, IdentityAuthResultDto>
{
    /// <summary>
    /// Processes the login command by validating credentials and generating authentication tokens.
    /// </summary>
    /// <param name="command">The login command containing user credentials</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authentication result with tokens</returns>
    public async Task<IdentityAuthResultDto> HandleAsync(IdentityLoginCommand command, CancellationToken cancellationToken = default)
    {
        logger.IdentityLoginStarted(command.Nameid);

        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Nameid);
        }
        catch (ArgumentException)
        {
            logger.IdentityLoginRejectedInvalidNameId(command.Nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var user = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (user is null)
        {
            logger.IdentityLoginRejectedUserNotFound(command.Nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (!user.IsActive)
        {
            logger.IdentityLoginRejectedUserNotActive(command.Nameid);

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

            throw new InvalidCredentialsFargoApplicationException();
        }

        var isValid = user.Authentication.PasswordHash != null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, password);

        if (!isValid)
        {
            logger.IdentityLoginRejectedInvalidPassword(user.Guid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (user.Authentication.IsPasswordChangeRequired)
        {
            logger.IdentityLoginRejectedPasswordChangeRequired(user.Guid);

            throw new UserPasswordChangeRequiredFargoApplicationException(user.Guid);
        }

        var accessTokenResult = tokenGenerator.Generate(user);

        var rawRefreshToken = refreshTokenGenerator.Generate();

        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.Guid, refreshTokenHash);

        refreshTokenRepository.Add(refreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResult = new IdentityAuthResultDto(
            accessTokenResult.AccessToken.Value, rawRefreshToken.Value, accessTokenResult.ExpiresAt);

        logger.IdentityLoginCompleted(command.Nameid);

        return authResult;
    }
}
