using Fargo.Application.Abstractions;
using Fargo.Application.Common;
using Fargo.Application.Operations.Identity.Dtos;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Identity;

/// <summary>
/// Command used to authenticate a user with a nameid and password.
/// </summary>
/// <param name="Nameid">
/// The unique user identifier used for login.
/// </param>
/// <param name="Password">
/// The plaintext password provided for authentication.
/// </param>
public sealed record IdentityLoginCommand(
    string Nameid,
    string Password
) : ICommand<IdentityAuthResultDto>;

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
    TimeProvider timeProvider
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
        var shouldLogin = await attemptService.IsAllowedAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

        if (!shouldLogin.Allow)
        {
            throw new AuthenticationRateLimitFargoApplicationException(shouldLogin.RetryAfter,
                $"Too many authentication attempts for user '{command.Nameid}'.");
        }

        Nameid nameid;

        try
        {
            nameid = new Nameid(command.Nameid);
        }
        catch (ArgumentException)
        {
            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            // Persist the recorded failed attempt before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException($"Nameid '{command.Nameid}' has an invalid format.");
        }

        var user = await userRepository.GetByNameidAsync(nameid, cancellationToken);

        if (user is null)
        {
            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            // Persist the recorded failed attempt before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException($"User '{command.Nameid}' was not found.");
        }

        if (!user.Actor.IsActive)
        {
            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            // Persist the recorded failed attempt before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException($"User '{command.Nameid}' is not active.");
        }

        Password password;

        try
        {
            password = new Password(command.Password);
        }
        catch (ArgumentException)
        {
            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            // Persist the recorded failed attempt before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException($"The password format is invalid for user '{user.ActorGuid}'.");
        }

        var isValid = user.Authentication.PasswordHash != null
            && passwordHasher.Verify(user.Authentication.PasswordHash.Value, password);

        if (!isValid)
        {
            await attemptService.RegisterFailureAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

            // Persist the recorded failed attempt before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new InvalidCredentialsFargoApplicationException($"The password does not match for user '{user.ActorGuid}'.");
        }

        if (user.Authentication.IsPasswordChangeRequiredAt(timeProvider.GetUtcNow()))
        {
            await attemptService.RegisterSuccessAsync(
                command.Nameid,
                clientContext.IpAddress,
                cancellationToken);

            // Persist the recorded attempt before throwing: the
            // commit-on-success decorator skips committing when the handler
            // throws, so this reject-path save must remain here.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UserPasswordChangeRequiredFargoApplicationException(user.ActorGuid);
        }

        var accessTokenResult = tokenGenerator.Generate(user);

        var rawRefreshToken = refreshTokenGenerator.Generate();

        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.ActorGuid, refreshTokenHash, timeProvider.GetUtcNow());

        refreshTokenRepository.Add(refreshToken);

        await attemptService.RegisterSuccessAsync(command.Nameid, clientContext.IpAddress, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResult = new IdentityAuthResultDto(
            accessTokenResult.AccessToken.Value, rawRefreshToken.Value, accessTokenResult.ExpiresAt);

        return authResult;
    }
}
