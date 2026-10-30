using Fargo.Application.Common;

namespace Fargo.Application.Identity;

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
