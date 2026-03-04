using Fargo.Application.Exceptions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserDeleteCommand(
            Guid UserGuid
            ) : ICommand;

    public sealed class UserDeleteCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser
            ) : ICommandHandler<UserDeleteCommand>
    {
        public async Task Handle(UserDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var actor = await userRepository.GetByGuid(
                    currentUser.UserGuid,
                    cancellationToken
                    ) ?? throw new UnauthorizedAccessFargoApplicationException();

            var user = await userRepository.GetByGuid(
                    command.UserGuid,
                    cancellationToken
                    ) ?? throw new UserNotFoundFargoApplicationException(command.UserGuid);

            actor.ValidatePermission(ActionType.DeleteUser);

            userRepository.Remove(user);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}