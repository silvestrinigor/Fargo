namespace Fargo.Application.Models.AuthModels
{
    public record AuthResultModel(
            string AccessToken,
            DateTime ExpiresAt
            );
}