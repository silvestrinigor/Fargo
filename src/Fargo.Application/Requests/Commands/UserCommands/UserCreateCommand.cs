using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Security;
using Fargo.Domain.Services.UserServices;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserCreateCommand(
            UserCreateModel User
            ) : ICommand<Guid>;

    public sealed class UserCreateCommandHandler(
            UserCreateService userCreateService,
            ActorGetService actorGetService,
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
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var userPasswordHash = passwordHasher.Hash(command.User.Password.Value);

            var user = userCreateService.CreateUser(
                    actor,
                    command.User.Nameid,
                    new PasswordHash(userPasswordHash)
                    );

            await unitOfWork.SaveChanges(cancellationToken);

            return user.Guid;
        }
    }
}