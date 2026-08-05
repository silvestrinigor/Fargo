using Fargo.Core.Security;
using Fargo.Core.Shared.Security;
using Microsoft.AspNetCore.Identity;

namespace Fargo.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public PasswordHash Hash(Password password)
    {
        var passwordHashString = _hasher.HashPassword(null!, password);

        return new(passwordHashString);
    }

    public bool Verify(PasswordHash hashedPassword, Password providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(
            null!,
            hashedPassword.Value,
            providedPassword
        );

        return result != PasswordVerificationResult.Failed;
    }
}
