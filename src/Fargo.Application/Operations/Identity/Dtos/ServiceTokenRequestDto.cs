namespace Fargo.Application.Operations.Identity.Dtos;

/// <summary>
/// Request body for the service-account client-credentials token endpoint.
/// </summary>
public sealed record ServiceTokenRequestDto(
    string ClientId,
    string ClientSecret
);
