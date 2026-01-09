using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserCreateCommand(
        UserCreateDto User
        ) : ICommand<Guid>;

    public sealed class UserCreateCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<UserCreateCommand, Guid>
    {
        private readonly IUserRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task<Guid> HandleAsync(UserCreateCommand command, CancellationToken cancellationToken = default)
        {
            var user = new User
            {
                Name = command.User.Name,
                Description = command.User.Description
            };

            repository.Add(user);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return user.Guid;
        }
    }
}
