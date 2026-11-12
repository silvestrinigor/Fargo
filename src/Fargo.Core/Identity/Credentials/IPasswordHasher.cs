namespace Fargo.Core.Identity.Credentials;

public interface IPasswordHasher
{
    PasswordHash Hash(Password password);

    bool Verify(PasswordHash hashedPassword, Password providedPassword);
}
