using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserDeleteCommand(
        Guid UserGuid
        ) : ICommand;

    public sealed class UserDeleteCommandHandler(
        UserService service,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserDeleteCommand>
    {
        public async Task Handle(UserDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var actor = currentUser.ToActor();

            var user = await service.GetUser(
                    actor,
                    command.UserGuid,
                    cancellationToken
                    );

            service.DeleteUser(actor, user);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}