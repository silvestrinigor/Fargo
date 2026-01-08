using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands
{
    public sealed record UserDeleteCommand(
        Guid UserGuid
        ) : ICommand;

    public sealed class UserDeleteCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<UserDeleteCommand>
    {
        private readonly IUserRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(UserDeleteCommand command, CancellationToken cancellationToken = default)
        {
            var user = await repository.GetByGuidAsync(command.UserGuid, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            repository.Remove(user);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
