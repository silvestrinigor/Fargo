using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserPermissionUpdateCommand(
        Guid UserGuid,
        PermissionUpdateModel Permission
        ) : ICommand;

    public sealed class UserPermissionUpdateCommandHandler(
        UserService service, 
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<UserPermissionUpdateCommand>
    {
        private readonly UserService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(UserPermissionUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var user = await service.GetUserAsync(command.UserGuid, cancellationToken);

            user.SetPermission(command.Permission.ActionType, command.Permission.GrantType);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
