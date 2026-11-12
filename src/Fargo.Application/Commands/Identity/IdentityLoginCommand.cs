using Fargo.Application.Abstractions;
using Fargo.Application.Common;
using Fargo.Application.Dtos.Identity;
using Fargo.Application.Exceptions.Identity;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Identity.Tokens;
using Fargo.Core.Informations;

namespace Fargo.Application.Commands.Identity;

public sealed record IdentityLoginCommand(
    string Nameid,
    string Password
) : ICommand<IdentityAuthResultDto>;

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
