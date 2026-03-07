using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Security
{
    /// <summary>
    /// Defines the contract for password hashing operations.
    ///
    /// Implementations are responsible for securely hashing plaintext
    /// passwords and verifying provided passwords against stored hashes.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Generates a secure hash for the specified password.
        /// </summary>
        /// <param name="password">The plaintext password.</param>
        /// <returns>A <see cref="PasswordHash"/> representing the hashed password.</returns>
        PasswordHash Hash(Password password);

        /// <summary>
        /// Verifies whether the provided password matches the stored hash.
        /// </summary>
        /// <param name="hashedPassword">The stored password hash.</param>
        /// <param name="providedPassword">The plaintext password provided for verification.</param>
        /// <returns>
        /// <see langword="true"/> if the password matches the hash; otherwise, <see langword="false"/>.
        /// </returns>
        bool Verify(PasswordHash hashedPassword, Password providedPassword);
    }
}