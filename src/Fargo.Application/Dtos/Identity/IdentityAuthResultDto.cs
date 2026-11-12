namespace Fargo.Application.Dtos.Identity;

public record IdentityAuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
