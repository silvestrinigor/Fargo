namespace Fargo.Application.Requests.Commands.AuthCommands
{
    public sealed record LoginCommand(

            ) : ICommand<Task>;

    public sealed class LoginCommandHandler(

            ) : ICommandHandler<LoginCommand, Task>
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