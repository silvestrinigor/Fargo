using Fargo.Application.Abstractions;
using Fargo.Application.Common;
using Fargo.Application.Operations.Identity.Dtos;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Identity.Authentication;
using Fargo.Core.Identity.Credentials;
using Fargo.Core.Informations;

namespace Fargo.Application.Operations.Identity;

/// <summary>
/// Command used to authenticate a service account via the client-credentials
/// flow and obtain a short-lived access token.
/// </summary>
/// <param name="ClientId">The service account's unique client identifier.</param>
/// <param name="ClientSecret">The plaintext client secret.</param>
public sealed record ServiceTokenCommand(
    string ClientId,
    string ClientSecret
) : ICommand<ServiceTokenResultDto>;

/// <summary>
/// Handles the service-account client-credentials flow: it enforces the same
/// authentication-attempt lockout users get, validates the client id and secret,
/// and issues a short-lived access token when the account exists, is active, and
/// the secret matches. The token carries a service actor-type claim so the rest
/// of the pipeline resolves the caller as an <c>ActorType.Service</c> actor.
/// </summary>
public sealed class ServiceTokenCommandHandler(
    IServiceAccountRepository serviceAccountRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    IAuthenticationAttemptService attemptService,
    IClientContext clientContext,
    IUnitOfWork unitOfWork
) : ICommandHandler<ServiceTokenCommand, ServiceTokenResultDto>
{
    public async Task<ServiceTokenResultDto> HandleAsync(ServiceTokenCommand command, CancellationToken cancellationToken = default)
    {
        var allowed = await attemptService.IsAllowedAsync(command.ClientId, clientContext.IpAddress, cancellationToken);

        if (!allowed.Allow)
        {
            throw new AuthenticationRateLimitFargoApplicationException(
                allowed.RetryAfter,
                $"Too many authentication attempts for client '{command.ClientId}'.");
        }

        Nameid clientId;

        try
        {
            clientId = new Nameid(command.ClientId);
        }
        catch (ArgumentException)
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException($"Client id '{command.ClientId}' has an invalid format.");
        }

        var service = await serviceAccountRepository.GetByClientIdAsync(clientId, cancellationToken);

        if (service is null || !service.Actor.IsActive)
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException($"Client '{command.ClientId}' was not found or is not active.");
        }

        Password secret;

        try
        {
            secret = new Password(command.ClientSecret);
        }
        catch (ArgumentException)
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException($"The client secret format is invalid for client '{command.ClientId}'.");
        }

        if (!passwordHasher.Verify(service.SecretHash, secret))
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException($"The client secret does not match for client '{command.ClientId}'.");
        }

        var tokenResult = tokenGenerator.Generate(service);

        await attemptService.RegisterSuccessAsync(command.ClientId, clientContext.IpAddress, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ServiceTokenResultDto(tokenResult.AccessToken.Value, tokenResult.ExpiresAt);
    }

    // Records a failed attempt and persists it immediately: the commit-on-success
    // decorator skips committing when the handler throws, so this reject-path save
    // must happen here (mirrors the user login handler).
    private async Task RegisterFailureAndSaveAsync(string clientId, CancellationToken cancellationToken)
    {
        await attemptService.RegisterFailureAsync(clientId, clientContext.IpAddress, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
