using Fargo.Application.Exceptions;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
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
            ITokenGenerator tokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            ITokenHasher tokenHasher,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork
            ) : ICommandHandler<LoginCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(
                LoginCommand command,
                CancellationToken cancellationToken = default
                )
        {
            var user = await repository.GetByNameid(
                    nameid: command.Nameid,
                    cancellationToken
                    )
                ?? throw new InvalidCredentialsException();

            var isValid = passwordHasher.Verify(
                    user.PasswordHash,
                    command.Password
                    );

            if (!isValid)
                throw new InvalidCredentialsException();

            var token = tokenGenerator.Generate(user);

            var rawRefresh = refreshTokenGenerator.Generate();

            var refreshHash = tokenHasher.Hash(rawRefresh);

            var refreshTokenStored = new RefreshToken
            {
                TokenHash = refreshHash,
                UserGuid = user.Guid
            };

            refreshTokenRepository.Add(refreshTokenStored);

            await unitOfWork.SaveChanges(cancellationToken);

            return new AuthResult(token.AccessToken, rawRefresh, token.ExpiresAt);
        }
    }
}