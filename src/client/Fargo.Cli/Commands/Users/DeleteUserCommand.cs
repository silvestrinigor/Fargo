using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class DeleteUserCommand : Command
{
    public DeleteUserCommand(FargoApiClient client)
        : base("delete", "Delete a user by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.Users[guid].DeleteAsync();

            Console.WriteLine("User deleted.");

            return 0;
        });
    }
}
