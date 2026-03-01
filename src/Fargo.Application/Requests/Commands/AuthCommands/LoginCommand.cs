using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Commands.AuthCommands
{
    public sealed record LoginCommand(
            Nameid Nameid,
            Password Password
            ) : ICommand<AuthResult>;

    public sealed class LoginCommandHandler(
            IUserRepository repository,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator
            ) : ICommandHandler<LoginCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(
                LoginCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var user = await repository.GetByNameid(
                    nameid: command.Nameid,
                    partitionGuids: null,
                    cancellationToken
                    )
                ?? throw new InvalidCredentialsException();

            var isValid = passwordHasher.Verify(
                    command.Password.Value,
                    user.PasswordHash.Value ?? string.Empty
                    );

            if (!isValid)
                throw new InvalidCredentialsException();

            var token = tokenGenerator.Generate(user);

            return token;
        }
    }
}