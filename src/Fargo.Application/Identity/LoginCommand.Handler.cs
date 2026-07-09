using Fargo.Application.Shared.Identity;
using Fargo.Core.Identity;
using Fargo.Core.Shared;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

public sealed class LoginCommandHandler(
    IUserRepository userRepository, IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator, IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher, IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork, ILogger<LoginCommandHandler> logger
) : ICommandHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> HandleAsync(
        LoginCommand command, CancellationToken cancellationToken = default)
    {
        logger.LoginStarted(command.Nameid);

        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Nameid);
        }
        catch (ArgumentException)
        {
            logger.LoginRejectedInvalidNameId(command.Nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var user = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (user is null)
        {
            logger.LoginRejectedUserNotFound(command.Nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (!user.IsActive)
        {
            logger.LoginRejectedUserNotActive(command.Nameid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        var isValid = passwordHasher.Verify(
            user.PasswordHash, command.Password);

        if (!isValid)
        {
            logger.LoginRejectedInvalidPassword(user.Guid);

            throw new InvalidCredentialsFargoApplicationException();
        }

        if (user.IsPasswordChangeRequired)
        {
            logger.LoginRejectedPasswordChangeRequired(user.Guid);

            throw new PasswordChangeRequiredException(user.Guid);
        }

        var accessTokenResult = tokenGenerator.Generate(user);

        var rawRefreshToken = refreshTokenGenerator.Generate();

        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = new RefreshToken
        {
            UserGuid = user.Guid,
            TokenHash = refreshTokenHash
        };

        refreshTokenRepository.Add(refreshToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResult = new AuthResult(
            accessTokenResult.AccessToken, rawRefreshToken, accessTokenResult.ExpiresAt);

        logger.LoginCompleted(command.Nameid);

        return authResult;
    }
}
