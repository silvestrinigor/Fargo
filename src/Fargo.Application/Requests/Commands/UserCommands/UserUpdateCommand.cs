using Fargo.Application.Exceptions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Security;
using Fargo.Domain.Services.UserServices;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserUpdateCommand(
            Guid UserGuid,
            UserUpdateModel User
            ) : ICommand;

    public sealed class UserUpdateCommandHandler(
            UserGetService userGetService,
            ActorGetService actorGetService,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<UserUpdateCommand>
    {
        public async Task Handle(
                UserUpdateCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var actor = await actorGetService.GetActor(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var user = await userGetService.GetUser(
                    actor,
                    command.UserGuid,
                    cancellationToken
                    ) ?? throw new UserNotFoundFargoApplicationException(command.UserGuid);

            user.Nameid = command.User.Nameid ?? user.Nameid;

            user.Description = command.User.Description ?? user.Description;

            if (command.User.Password != null)
            {
                var userPasswordHash = passwordHasher.Hash(command.User.Password.NewPassword.Value);

                user.PasswordHash = new PasswordHash(userPasswordHash);
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}