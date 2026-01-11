using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fargo.Application.Requests.Commands.UserCommands
{
    public sealed record UserUpdateCommand(
        Guid UserGuid,
        UserUpdateModel User
        ) : ICommand;

    public sealed class UserUpdateCommandHandler(
        IUserRepository repository, IUnitOfWork unitOfWork
        ) : ICommandHandlerAsync<UserUpdateCommand>
    {
        private readonly IUserRepository repository = repository;

        private readonly IUnitOfWork unitOfWork = unitOfWork;

        public async Task HandleAsync(UserUpdateCommand command, CancellationToken cancellationToken = default)
        {
            var user = await repository.GetByGuidAsync(command.UserGuid, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            user.Name = command.User.Name ?? user.Name;

            user.Description = command.User.Description ?? user.Description;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
