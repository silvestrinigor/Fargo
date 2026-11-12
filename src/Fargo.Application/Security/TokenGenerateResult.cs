using Fargo.Core.Identity.Tokens;

namespace Fargo.Application.Security;

public record TokenGenerateResult(
    Token AccessToken,
    DateTimeOffset ExpiresAt
);
