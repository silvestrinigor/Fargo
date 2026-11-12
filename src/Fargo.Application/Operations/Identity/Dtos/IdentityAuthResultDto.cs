namespace Fargo.Application.Operations.Identity.Dtos;

public record IdentityAuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
