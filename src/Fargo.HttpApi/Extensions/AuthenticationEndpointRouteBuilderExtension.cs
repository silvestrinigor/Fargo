using Fargo.Application.Models.AuthModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.AuthCommands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    public static class AuthenticationEndpointRouteBuilderExtension
    {
        public static void MapFargoAuthentication(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/authentication");

            group.MapPost("/login", Login);
        }

        private static async Task<Ok<AuthResult>> Login(
            LoginCommand command,
            ICommandHandler<LoginCommand, AuthResult> handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(command, cancellationToken);

            return TypedResults.Ok(result);
        }
    }
}