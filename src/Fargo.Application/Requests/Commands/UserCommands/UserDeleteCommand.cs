using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services.UserServices;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserDeleteCommand(
            Guid UserGuid
            ) : ICommand;

    public sealed class UserDeleteCommandHandler(
            UserDeleteService userDeleteService,
            UserGetService userGetService,
            ActorGetService actorGetService,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<UserDeleteCommand>
    {
        public async Task Handle(UserDeleteCommand command, CancellationToken cancellationToken = default)
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

            userDeleteService.DeleteUser(actor, user);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}