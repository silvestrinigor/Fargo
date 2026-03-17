using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.AuthCommands;

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
        Nameid Nameid,
        Password Password
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
        IUnitOfWork unitOfWork
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
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
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
        var user = await userRepository.GetByNameid(
                command.Nameid,
                cancellationToken
                ) ?? throw new UnauthorizedAccessFargoApplicationException();

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        var isValid = passwordHasher.Verify(
                user.PasswordHash,
                command.Password
                );

        if (!isValid)
        {
            throw new UnauthorizedAccessFargoApplicationException();
        }

        if (user.IsPasswordChangeRequired)
        {
            throw new PasswordChangeRequiredFargoApplicationException(user.Guid);
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

        await unitOfWork.SaveChanges(cancellationToken);

        return new AuthResult(
                accessTokenResult.AccessToken,
                rawRefreshToken,
                accessTokenResult.ExpiresAt
                );
    }
}
