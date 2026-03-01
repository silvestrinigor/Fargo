namespace Fargo.Application.Models.AuthModels
{
    public record AuthResult(
            string AccessToken,
            DateTimeOffset ExpiresAt
            );
}