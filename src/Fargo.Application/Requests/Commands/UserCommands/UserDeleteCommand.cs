using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserDeleteCommand(
        Guid UserGuid
        ) : ICommand;

    public sealed class UserDeleteCommandHandler(
        UserService service, 
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<UserDeleteCommand>
    {
        private readonly UserService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(UserDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var user = await service.GetUserAsync(command.UserGuid, cancellationToken);

            service.DeleteUser(user);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
