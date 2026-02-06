using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserUpdateCommand(
        Guid UserGuid,
        UserUpdateModel User
        ) : ICommand<Task>;

    public sealed class UserUpdateCommandHandler(
        UserService service,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<UserUpdateCommand, Task>
    {
        private readonly UserService service = service;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task Handle(UserUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var user = await service.GetUserAsync(command.UserGuid, cancellationToken);

            user.Name = command.User.Name ?? user.Name;

            user.Description = command.User.Description ?? user.Description;

            if (command.User.Password != null)
            {
                service.SetPassword(
                        user,
                        new(command.User.Password.NewPassword),
                        new(command.User.Password.CurrentPassword)
                        );
            }

            await unitOfWork.SaveChanges(cancellationToken);
        }
    }
}