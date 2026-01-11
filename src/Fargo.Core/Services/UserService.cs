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

        public User CreateUser(Name name, Description description, Password password)
        {
            var user = new User
            {
                Name = name,
                Description = description,
                PasswordHash = new(passwordHasher.Hash(password.Value)),
            };

            repository.Add(user);

            return user;
        }
    }
}
