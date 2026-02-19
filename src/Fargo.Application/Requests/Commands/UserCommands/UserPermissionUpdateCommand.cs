using Fargo.Application.Extensions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserPermissionUpdateCommand(
        Guid UserGuid,
        PermissionUpdateModel Permission
        ) : ICommand;

    public sealed class UserPermissionUpdateCommandHandler(
        UserService service,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserPermissionUpdateCommand>
    {
        public async Task Handle(UserPermissionUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var actor = currentUser.ToActor();

            var user = await service.GetUser(
                    actor,
                    command.UserGuid,
                    cancellationToken
                    );

            user.SetPermission(command.Permission.ActionType, command.Permission.GrantType);

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}