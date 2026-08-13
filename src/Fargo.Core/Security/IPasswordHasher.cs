namespace Fargo.Core.Security;

/// <summary>
/// Defines the contract for password hashing operations.
/// </summary>
/// <remarks>
/// Implementations are responsible for securely hashing plaintext passwords
/// and verifying provided passwords against stored password hashes.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Generates a secure hash for the specified password.
    /// </summary>
    /// <param name="password">
    /// The plaintext password to hash.
    /// </param>
    /// <returns>
    /// A <see cref="PasswordHash"/> representing the hashed password.
    /// </returns>
    PasswordHash Hash(Password password);

    /// <summary>
    /// Verifies whether the provided password matches the stored hash.
    /// </summary>
    /// <param name="hashedPassword">
    /// The stored password hash.
    /// </param>
    /// <param name="providedPassword">
    /// The plaintext password to verify.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the provided password matches the stored hash;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Verify(PasswordHash hashedPassword, Password providedPassword);
}
