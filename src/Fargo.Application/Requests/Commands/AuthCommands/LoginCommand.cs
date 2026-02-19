namespace Fargo.Application.Requests.Commands.AuthCommands
{
    public sealed record LoginCommand(

            ) : ICommand;

    public sealed class LoginCommandHandler(

            ) : ICommandHandler<LoginCommand>
    {
        public async Task Handle(
                LoginCommand command,
                CancellationToken cancellationToken = default
                )
        {
            throw new NotImplementedException();
        }
    }
}