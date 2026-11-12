namespace Fargo.Core.Identity.Tokens;

/// <summary>
/// Generates cryptographically secure random refresh tokens.
/// </summary>
public interface IRefreshTokenGenerator
{
    Token Generate();
}
