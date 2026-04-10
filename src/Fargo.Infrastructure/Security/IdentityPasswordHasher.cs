using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Fargo.Infrastructure.Security;

/// <summary>
/// Provides password hashing and verification using ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// This implementation delegates hashing and verification to
/// <see cref="PasswordHasher{TUser}"/>, using <see cref="object"/> as the
/// user type because the hashing process does not depend on a specific user instance.
///
/// The generated hash format, compatibility mode, and verification behavior
/// are defined by the underlying ASP.NET Core Identity implementation.
/// </remarks>
public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    /// <summary>
    /// Hashes the specified password.
    /// </summary>
    /// <param name="password">
    /// The plain text password string to hash.
    /// </param>
    /// <returns>
    /// A <see cref="PasswordHash"/> containing the hashed representation
    /// of the provided password.
    /// </returns>
    public PasswordHash Hash(string password)
    {
        var passwordHashString = _hasher.HashPassword(null!, password);

        return new PasswordHash(passwordHashString);
    }

    /// <summary>
    /// Verifies whether a provided password matches a previously hashed password.
    /// </summary>
    /// <param name="hashedPassword">
    /// The stored password hash.
    /// </param>
    /// <param name="providedPassword">
    /// The plain text password string supplied for verification.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the provided password matches the stored hash;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method returns <see langword="true"/> for both
    /// <see cref="PasswordVerificationResult.Success"/> and
    /// <see cref="PasswordVerificationResult.SuccessRehashNeeded"/>,
    /// since both indicate that the password is valid.
    /// </remarks>
    public bool Verify(PasswordHash hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(
            null!,
            hashedPassword,
            providedPassword
        );

        return result != PasswordVerificationResult.Failed;
    }
}
