using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Commands
{
    public sealed record UserSetPermissionCommand(
        Guid UserGuid,
        UserPermissionDto Permission
        ) : ICommand;

    public sealed class UserSetPermissionCommandHandler(IUserRepository repository, IUnitOfWork unitOfWork) : ICommandHandlerAsync<UserSetPermissionCommand>
    {
        private readonly IUserRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(UserSetPermissionCommand command, CancellationToken cancellationToken = default)
        {
            var user = await repository.GetByGuidAsync(command.UserGuid, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            user.SetPermission(command.Permission.ActionType, command.Permission.GrantType);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
