using Fargo.Domain.Security;
using Microsoft.AspNetCore.Identity;

namespace Fargo.Infrastructure.Security
{
    public sealed class IdentityPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public bool Verify(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                null!,
                hashedPassword,
                providedPassword
            );

            return result != PasswordVerificationResult.Failed;
        }
    }
}