namespace Fargo.Application.Operations.Identity.Dtos;

public sealed record ServiceTokenRequestDto(
    string ClientId,
    string ClientSecret
);
