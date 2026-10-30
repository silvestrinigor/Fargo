using Fargo.Application.Common;
using Fargo.Core.ServiceAccounts;
using Fargo.Core.Identity;
using Fargo.Core.Informations;
using Fargo.Core.Security;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Identity;

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
    IUnitOfWork unitOfWork,
    ILogger<ServiceTokenCommandHandler> logger
) : ICommandHandler<ServiceTokenCommand, ServiceTokenResultDto>
{
    public async Task<ServiceTokenResultDto> HandleAsync(ServiceTokenCommand command, CancellationToken cancellationToken = default)
    {
        var allowed = await attemptService.IsAllowedAsync(command.ClientId, clientContext.IpAddress, cancellationToken);

        if (!allowed.Allow)
        {
            logger.ServiceTokenRejectedRateLimit(command.ClientId);

            throw new AuthenticationRateLimitFargoApplicationException(allowed.RetryAfter);
        }

        Nameid clientId;

        try
        {
            clientId = new Nameid(command.ClientId);
        }
        catch (ArgumentException)
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException();
        }

        var service = await serviceAccountRepository.GetByClientIdAsync(clientId, cancellationToken);

        if (service is null || !service.IsActive)
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException();
        }

        Password secret;

        try
        {
            secret = new Password(command.ClientSecret);
        }
        catch (ArgumentException)
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException();
        }

        if (!passwordHasher.Verify(service.SecretHash, secret))
        {
            await RegisterFailureAndSaveAsync(command.ClientId, cancellationToken);
            throw new InvalidCredentialsFargoApplicationException();
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
        logger.ServiceTokenRejectedInvalidCredentials(clientId);

        await attemptService.RegisterFailureAsync(clientId, clientContext.IpAddress, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal static partial class ServiceTokenCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Service token flow rejected because too many authentication attempts for client '{clientId}'.")]
    public static partial void ServiceTokenRejectedRateLimit(this ILogger logger, string clientId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Service token flow rejected due to invalid credentials for client '{clientId}'.")]
    public static partial void ServiceTokenRejectedInvalidCredentials(this ILogger logger, string clientId);
}