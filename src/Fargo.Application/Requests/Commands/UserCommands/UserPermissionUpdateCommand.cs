using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories.UserRepositories;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserPermissionUpdateCommand(
        Guid UserGuid,
        UserPermissionDto Permission
        ) : ICommand;

    public sealed class UserPermissionUpdateCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<UserPermissionUpdateCommand>
    {
        private readonly IUserRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(UserPermissionUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var user = await repository.GetByGuidAsync(command.UserGuid, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            user.SetPermission(command.Permission.ActionType, command.Permission.GrantType);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
