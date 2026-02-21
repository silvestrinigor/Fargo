using Fargo.Application.Models.AuthModels;

namespace Fargo.Application.Requests.Commands.AuthCommands
{
    public sealed record LoginCommand(
            int Id,
            string Password
            ) : ICommand<AuthResultModel>;

    public sealed class LoginCommandHandler(

            ) : ICommandHandler<LoginCommand, AuthResultModel>
    {
        public async Task<AuthResultModel> Handle(
                LoginCommand command,
                CancellationToken cancellationToken = default
                )
        {
            throw new NotImplementedException();
        }
    }
}