using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Services;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserCreateCommand(
        UserCreateModel User
        ) : ICommand<Guid>;

    public sealed class UserCreateCommandHandler(
        UserService userService,
        IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<UserCreateCommand, Guid>
    {
        private readonly UserService userService = userService;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(UserCreateCommand command, CancellationToken cancellationToken = default)
        {
            var user = userService.CreateUser(
                command.User.Id,
                command.User.Name,
                command.User.Description,
                command.User.Password);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return user.Guid;
        }
    }
}
