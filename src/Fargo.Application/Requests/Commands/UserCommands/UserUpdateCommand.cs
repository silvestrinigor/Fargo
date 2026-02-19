using Fargo.Application.Extensions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserUpdateCommand(
        Guid UserGuid,
        UserUpdateModel User
        ) : ICommand;

    public sealed class UserUpdateCommandHandler(
        UserService service,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser
        ) : ICommandHandler<UserUpdateCommand>
    {
        public async Task Handle(UserUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var actor = currentUser.ToActor();

            var user = await service.GetUser(actor, command.UserGuid, cancellationToken);

            user.Name = command.User.Name ?? user.Name;

            user.Description = command.User.Description ?? user.Description;

            if (command.User.Password != null)
            {
                service.SetPassword(
                        actor,
                        user,
                        new(command.User.Password.NewPassword),
                        new(command.User.Password.CurrentPassword)
                        );
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}