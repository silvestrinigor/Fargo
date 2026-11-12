using Fargo.Core.Identity.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// The hash is an uppercase hexadecimal string, safe to store without exposing the token.
/// </summary>
public sealed class Sha256TokenHasher : ITokenHasher
{
    public TokenHash Hash(Token token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Value));

        var tokenHexStringHash = Convert.ToHexString(bytes);

        return new TokenHash(tokenHexStringHash);
    }
}
