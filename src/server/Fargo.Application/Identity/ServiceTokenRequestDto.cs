namespace Fargo.Application.Identity;

/// <summary>
/// Request body for the service-account client-credentials token endpoint.
/// </summary>
public sealed record ServiceTokenRequestDto(
    string ClientId,
    string ClientSecret
);
