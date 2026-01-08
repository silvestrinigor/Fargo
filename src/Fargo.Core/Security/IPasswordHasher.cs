using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hashedPassword, string providedPassword);
    }
}