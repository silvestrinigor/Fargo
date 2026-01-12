using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public sealed class UserService(IUserRepository repository, IPasswordHasher passwordHasher)
    {
        private readonly IUserRepository repository = repository;

        private readonly IPasswordHasher passwordHasher = passwordHasher;

        public User CreateUser(int id, Name name, Description description, Password password)
        {
            var user = new User
            {
                Id = id,
                Name = name,
                Description = description,
                PasswordHash = new(passwordHasher.Hash(password.Value)),
            };

            repository.Add(user);

            return user;
        }

        public void SetPassword(User user, Password newPassword, Password currentPassword)
        {
            var isPasswordCorrect = passwordHasher.Verify(user.PasswordHash.Value, currentPassword.Value);

            if (!isPasswordCorrect)
                throw new InvalidOperationException("Cannot set new password because the current password is incorrect.");

            user.PasswordHash = new(passwordHasher.Hash(newPassword.Value));
        }
    }
}
