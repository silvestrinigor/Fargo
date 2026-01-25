using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserCreateCommand(
        UserCreateModel User
        ) : ICommand<Task<Guid>>;

    public sealed class UserCreateCommandHandler(
        UserService userService,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<UserCreateCommand, Task<Guid>>
    {
        private readonly UserService userService = userService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> Handle(UserCreateCommand command, CancellationToken cancellationToken = default)
        {
            var user = userService.CreateUser(
                command.User.Id,
                command.User.Name,
                command.User.Description ?? default,
                command.User.Password);

            await unitOfWork.SaveChanges(cancellationToken);

            return user.Guid;
        }
    }
}