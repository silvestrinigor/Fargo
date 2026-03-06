using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using System.Security.Cryptography;

namespace Fargo.Infrastructure.Security
{
    public sealed class CryptoRefreshTokenGenerator : IRefreshTokenGenerator
    {
        public Token Generate()
        {
            var tokenString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return new Token(tokenString);
        }
    }
}