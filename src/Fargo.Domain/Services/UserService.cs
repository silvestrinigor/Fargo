using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Services
{
    public sealed class UserService(
            IUserRepository repository,
            IPasswordHasher passwordHasher
            )
    {
        private const string AdminGuidString = "";

        public static readonly Guid AdminGuid = new(AdminGuidString);

        public async Task<User> GetUser(
                Actor actor,
                Guid userGuid,
                CancellationToken cancellationToken = default
                )
            => await repository.GetByGuid(
                    userGuid,
                    actor.PartitionGuids,
                    cancellationToken
                    )
            ?? throw new UserNotFoundException(userGuid);

        public User CreateUser(
                Actor actor,
                int id,
                Name name,
                Description description,
                Password password
                )
        {
            if (!actor.HasPermission(ActionType.CreateUser))
            {
                throw new ActorNotAuthorizedException(
                        actor,
                        ActionType.CreateUser
                        );
            }

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

        public void DeleteUser(
                Actor actor,
                User user
                )
        {
            if (!actor.HasPermission(ActionType.CreateUser))
            {
                throw new ActorNotAuthorizedException(
                        actor,
                        ActionType.CreateUser
                        );
            }

            repository.Remove(user);
        }

        public void SetPassword(
                Actor actor,
                User user,
                Password newPassword,
                Password currentPassword
                )
        {
            if (actor.UserGuid != user.Guid)
            {
                throw new UserActorSetDiferentUserPasswordException(
                        actor,
                        user
                        );
            }

            var isPasswordCorrect = passwordHasher.Verify(
                    user.PasswordHash.Value,
                    currentPassword.Value
                    );

            if (!isPasswordCorrect)
            {
                throw new UserInvalidPasswordException(user);
            }

            user.PasswordHash = new(passwordHasher.Hash(newPassword.Value));
        }
    }
}