using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserCreateCommand(
            UserCreateModel User
            ) : ICommand<Guid>;

    public sealed class UserCreateCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher
            ) : ICommandHandler<UserCreateCommand, Guid>
    {
        public async Task<Guid> Handle(
                UserCreateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    partitionGuids: null,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var userPasswordHash = passwordHasher.Hash(command.User.Password);

            actor.ValidatePermission(ActionType.CreateUser);

            var user = new User
            {
                Nameid = command.User.Nameid,
                PasswordHash = new(userPasswordHash),
                Permissions = command.User.Permissions ?? [],
                UpdatedBy = actor
            };

            userRepository.Add(user);

            await unitOfWork.SaveChanges(cancellationToken);

            return user.Guid;
        }
    }
}