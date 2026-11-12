namespace Fargo.Core.Identity.Tokens;

/// <summary>
/// Produces a deterministic hash, so the hash can be stored and looked up instead of the token.
/// </summary>
public interface ITokenHasher
{
    TokenHash Hash(Token token);
}
