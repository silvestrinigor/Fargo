using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.AuthCommands;

namespace Fargo.HttpApi.Extensions
{
    public static class AuthenticationEndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoAuthentication()
            {
                builder.MapPost(
                        "/authentication/login",
                        async (
                            LoginCommand command,
                            ICommandHandler<LoginCommand> handler,
                            CancellationToken cancellationToken
                            ) =>
                        {
                        await handler.Handle(command, cancellationToken);

                        return TypedResults.Ok();
                        });
            }
        }
    }
}