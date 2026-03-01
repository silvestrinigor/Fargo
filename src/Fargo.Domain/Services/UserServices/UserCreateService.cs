using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services.UserServices
{
    public class UserCreateService(
            IUserRepository userRepository
            )
    {
        public User CreateUser(
                User actor,
                Nameid nameid,
                PasswordHash passwordHash
                )
        {
            actor.ValidatePermission(ActionType.CreateUser);

            var user = new User
            {
                Nameid = nameid,
                PasswordHash = passwordHash,
                UpdatedBy = actor
            };

            userRepository.Add(user);

            return user;
        }
    }
}