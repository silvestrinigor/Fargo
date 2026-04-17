using Fargo.Domain.Tokens;
using System.Security.Cryptography;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Generates cryptographically secure refresh tokens.
/// </summary>
/// <remarks>
/// This implementation uses <see cref="RandomNumberGenerator"/> to produce
/// a sequence of random bytes and encodes the result as a Base64 string.
///
/// The generated token is intended for authentication and session renewal
/// scenarios where unpredictability is required.
/// </remarks>
public sealed class CryptoRefreshTokenGenerator : IRefreshTokenGenerator
{
    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>
    /// A new <see cref="Token"/> containing a cryptographically secure
    /// random value encoded as a Base64 string.
    /// </returns>
    public Token Generate()
    {
        var tokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new Token(tokenString);
    }
}
