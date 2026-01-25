using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserPermissionUpdateCommand(
        Guid UserGuid,
        PermissionUpdateModel Permission
        ) : ICommand<Task>;

    public sealed class UserPermissionUpdateCommandHandler(
        UserService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<UserPermissionUpdateCommand, Task>
    {
        private readonly UserService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(UserPermissionUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var user = await service.GetUserAsync(command.UserGuid, cancellationToken);

            user.SetPermission(command.Permission.ActionType, command.Permission.GrantType);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}