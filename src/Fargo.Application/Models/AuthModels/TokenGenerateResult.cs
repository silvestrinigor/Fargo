using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Models.AuthModels
{
    public record TokenGenerateResult(
            Token AccessToken,
            DateTimeOffset ExpiresAt
            );
}