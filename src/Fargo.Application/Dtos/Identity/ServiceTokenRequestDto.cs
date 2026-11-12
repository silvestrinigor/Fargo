namespace Fargo.Application.Dtos.Identity;

public sealed record ServiceTokenRequestDto(
    string ClientId,
    string ClientSecret
);
