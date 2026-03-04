using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Security
{
    public interface IPasswordHasher
    {
        PasswordHash Hash(Password password);

        bool Verify(PasswordHash hashedPassword, Password providedPassword);
    }
}