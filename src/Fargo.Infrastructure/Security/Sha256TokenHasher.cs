using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Text;

namespace Fargo.Infrastructure.Security
{
    public class Sha256TokenHasher : ITokenHasher
    {
        public TokenHash Hash(Token token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            var tokenHexStringHash = Convert.ToHexString(bytes);

            return new TokenHash(tokenHexStringHash);
        }
    }
}